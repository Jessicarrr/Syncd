using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Syncd.logging;
using Syncd.Models.Network.Payload;
using Syncd.Models.Network.Rooms.Interface;
using Syncd.Models.Network.StateUpdates.Impl;
using Syncd.Models.Playlist;
using Syncd.Models.Users;
using Syncd.Models.Users.Enum;
using Syncd.Models.Users.Interface;

namespace Syncd.Models
{
    public class Room : IDisposable
    {
        private bool disposed = false;
        private SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        public string id { get; set; }
        public string name { get; set; }

        public string currentYoutubeVideoId { get; set; }

        public string currentYoutubeVideoTitle { get; set; }
        public IPlaylistManager PlaylistManager { get; set; }
        public Models.Users.Interface.IUserManager UserManager { get; set; }
        public IConnectionManager ConnectionManager { get; set; }

        public double videoTimeSeconds { get; set; }

        private long lastTimeChange = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        private long lastCheck = 0;

        public int periodicTaskMilliseconds = 2000;
        private long periodicRemoveUsersMilliseconds = 15000;
        private long periodicSyncUsersVideosSeconds = 10;
        private DateTime lastTimeRemovedDisconnectedUsers = DateTime.Now;
        private DateTime lastTimeSyncedUsersVideos = DateTime.Now;
        private CancellationTokenSource source;

        private CancellationTokenSource destroyRoomTokenSource;
        private int timeUntilEmptyRoomDestroyedMS = 300000; // 300 seconds, or 5 minutes
        public bool RoomShouldBeDestroyed { get; private set; }

        private long playNewVideoAfterEndingDelayMS = 5000;
        private bool isTransitioningToNewVideo = false;
        private CancellationTokenSource cancelTransitioningToNewVideoToken = new CancellationTokenSource();

        public long roomCreationTime;

        private ILogger logger;


        public Room(string id, string name = "")
        {
            this.id = id;
            this.name = name;

            currentYoutubeVideoId = "LXb3EKWsInQ";
            currentYoutubeVideoTitle = "";
            videoTimeSeconds = 0;

            PlaylistManager = new PlaylistManager();
            UserManager = new Models.Users.Impl.UserManager(id);
            roomCreationTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            logger = LoggingHandler.CreateLogger<Room>();

            StartPeriodicTasks();
            StartEmptyRoomCountDown();
        }

        public Room(IPlaylistManager playlistManager, IUserManager userManager, IConnectionManager connectionManager,
            string id, string name = "")
        {
            this.id = id;
            this.name = name;
            
            currentYoutubeVideoId = "LXb3EKWsInQ";
            currentYoutubeVideoTitle = "";
            videoTimeSeconds = 0;

            roomCreationTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            this.UserManager = userManager;
            this.PlaylistManager = playlistManager;
            this.ConnectionManager = connectionManager;
            logger = LoggingHandler.CreateLogger<Room>();

            StartPeriodicTasks();
            StartEmptyRoomCountDown();
        }

        private void StartPeriodicTasks()
        {
            source = new CancellationTokenSource();
            var task = Task.Run(async () => {

                await HandlePeriodicTasks(source.Token);

            },
            source.Token);
        }

        private void CancelPeriodicTasks()
        {
            logger.LogInformation("[VSY]CancelPeriodicTasks() ran on room " + id);
            source.Cancel();
        }

        public async Task HandlePeriodicTasks(CancellationToken token)
        {
            while (true)
            {
                if(token.IsCancellationRequested)
                {
                    break;
                }

                await SyncUsersVideos(token);
                await PeriodicRemoveDisconnectedUsers(token);
                new Task(() => PeriodicUpdateVideoStatistics()).Start();
                await Task.Delay(periodicTaskMilliseconds);
            }
        }

        public async Task SyncUsersVideos(CancellationToken token)
        {
            TimeSpan elapsed = DateTime.Now - lastTimeSyncedUsersVideos;

            if(elapsed.TotalSeconds > periodicSyncUsersVideosSeconds)
            {
                lastTimeSyncedUsersVideos = DateTime.Now;

                if(UserManager.SomeoneHasState(VideoState.Ended))
                {
                    return;
                }

                VideoStatePayload payload = new VideoStatePayload()
                {
                    currentVideoState = GetSuggestedVideoState(),
                    currentYoutubeVideoId = currentYoutubeVideoId,
                    videoTimeSeconds = videoTimeSeconds,
                    currentYoutubeVideoTitle = currentYoutubeVideoTitle
                };

                RoomDataUpdate update = new RoomDataUpdate()
                {
                    updateType = Network.StateUpdates.Enum.UpdateType.VideoUpdate,
                    payload = payload
                };

                await ConnectionManager.SendUpdateToAll(this, update, token);
            }
        }

        public async Task PeriodicRemoveDisconnectedUsers(CancellationToken token)
        {
            TimeSpan elapsed = DateTime.Now - lastTimeRemovedDisconnectedUsers;

            if (elapsed.TotalMilliseconds >= periodicRemoveUsersMilliseconds)
            {
                int usersRemoved = await ConnectionManager.CheckAndRemoveDisconnectedUsers(this, token);
                lastTimeRemovedDisconnectedUsers = DateTime.Now;

                if (usersRemoved > 0 && UserManager.SomeoneHasState(VideoState.Ended) && !isTransitioningToNewVideo)
                {
                    isTransitioningToNewVideo = true;

                    //TODO: Playlist support, play next video.
                    PlaylistObject newVideoObj = PlaylistManager.GoToNextVideo();
                    NewVideo(newVideoObj);

                    VideoStatePayload payload = new VideoStatePayload()
                    {
                        currentVideoState = GetSuggestedVideoState(),
                        currentYoutubeVideoId = currentYoutubeVideoId,
                        videoTimeSeconds = videoTimeSeconds,
                        currentYoutubeVideoTitle = currentYoutubeVideoTitle
                    };

                    RoomDataUpdate update = new RoomDataUpdate()
                    {
                        updateType = Network.StateUpdates.Enum.UpdateType.VideoUpdate,
                        payload = payload
                    };

                    await ConnectionManager.SendUpdateToAll(this, update, token);
                    isTransitioningToNewVideo = false;
                }
            }
        }

        public void PeriodicUpdateVideoStatistics()
        {
            UpdateVideoStatistics();
        }

        public void NewVideo(string youtubeId)
        {
            currentYoutubeVideoId = youtubeId;
            currentYoutubeVideoTitle = "";
            UserManager.SetStateForAll(VideoState.Playing);
            videoTimeSeconds = 0;
        }

        public bool PlayPlaylistVideo(string playlistId)
        {
            if(isTransitioningToNewVideo)
            {
                cancelTransitioningToNewVideoToken.Cancel();
            }

            PlaylistObject obj = PlaylistManager.PlayPlaylistObject(playlistId);

            if(obj != null)
            {
                NewVideo(obj);
                return true;
            }
            return false;
        }

        public void NewVideo(PlaylistObject obj)
        {
            if(obj == null)
            {
                return;
            }
            currentYoutubeVideoId = obj.videoId;
            currentYoutubeVideoTitle = obj.title;
            UserManager.SetStateForAll(VideoState.Playing);
            videoTimeSeconds = 0;
        }

        public VideoState GetSuggestedVideoState()
        {
            if (UserManager.GetNumUsers() == 0)
            {
                return VideoState.Unstarted;
            }
            else
            {
                return UserManager.GetUserList().First().videoState;
            }
        }
        public bool NewVideoState(int userId, VideoState newState)
        {
            bool returnVal = false;

            if (newState == VideoState.Paused)
            {
                UserManager.SetStateForAll(VideoState.Paused);
                returnVal = true;
            }
            else if (newState == VideoState.Playing)
            {
                if(GetSuggestedVideoState() == VideoState.Ended)
                {
                    videoTimeSeconds = 0;
                }
                UserManager.SetStateForAll(newState);
                returnVal = true;
                
            }
            else if(newState == VideoState.Ended)
            {
                UserManager.SetStateForUser(userId, VideoState.Ended);

                if(!isTransitioningToNewVideo)
                {
                    isTransitioningToNewVideo = true;
                    returnVal = false;
                    cancelTransitioningToNewVideoToken = new CancellationTokenSource();
                    #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(async () =>
                    {
                        await Task.Delay((int)playNewVideoAfterEndingDelayMS);

                        if(!cancelTransitioningToNewVideoToken.Token.IsCancellationRequested)
                        {
                            try
                            {
                                PlaylistObject newVideo = PlaylistManager.GoToNextVideo();

                                if (newVideo != null)
                                {
                                    NewVideo(newVideo);

                                    VideoStatePayload payload = new VideoStatePayload()
                                    {
                                        currentVideoState = GetSuggestedVideoState(),
                                        currentYoutubeVideoId = currentYoutubeVideoId,
                                        videoTimeSeconds = videoTimeSeconds,
                                        currentYoutubeVideoTitle = currentYoutubeVideoTitle
                                    };

                                    RoomDataUpdate update = new RoomDataUpdate()
                                    {
                                        updateType = Network.StateUpdates.Enum.UpdateType.VideoUpdate,
                                        payload = payload
                                    };

                                    await ConnectionManager.SendUpdateToAll(this, update, cancelTransitioningToNewVideoToken.Token);
                                }
                            }
                            catch(Exception e)
                            {
                                logger.LogError("[VSY] Exception in autoplaying video in Room.cs. error:\n" + e.Message + "\n" + e.StackTrace);
                                Trace.TraceError("[VSY] Exception in autoplaying video in Room.cs. error:\n" + e.Message + "\n" + e.StackTrace);
                            }
                            finally
                            {
                                isTransitioningToNewVideo = false;
                            }
                        }
                        isTransitioningToNewVideo = false;

                    });
                    #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed


                }
                

                /*if(UserManager.AllHasState(VideoState.Ended))
                {
                    UpdateTime();

                    //TODO: Playlist support, play next video.
                    PlaylistObject obj = PlaylistManager.GoToNextVideo();
                    NewVideo(obj);
                    returnVal = true;
                }
                else
                {
                    returnVal = false;
                }*/
            }
            UpdateVideoStatistics();

            UserManager.UpdateLastConnectionTime(userId);

            /*
            else if (newState == VideoState.Unstarted)
            {
                currentVideoState = VideoState.Playing;
            }
            else
            {
                currentVideoState = VideoState.Paused;
            }*/
            return returnVal;
        }


        private void UpdateTime()
        {
            long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (lastCheck == 0)
            {
                lastCheck = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                return;
            }

            long timeSinceLastCheck = currentTime - lastCheck;
            double timeSinceLastCheckD = Convert.ToDouble(timeSinceLastCheck);
            double timeToAdd = timeSinceLastCheckD / 1000;
            videoTimeSeconds += timeToAdd;
            //CTrace.WriteLine("Added " + timeToAdd + " to video time for a total of " + videoTimeSeconds);
            lastCheck = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public void UpdateVideoStatistics()
        {
            if (GetSuggestedVideoState() == VideoState.Playing)
            {
                // Keep the server's interpretation of the video's time up to date with
                // the passing of time. This is so the server doesn't lag behind the client
                // and their playing video.

                UpdateTime();
            }
            else
            {
                lastCheck = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                //CTrace.WriteLine("Video is not playing. Video is " + GetSuggestedVideoState());
            }
        }

        public void TimeUpdate(double seconds)
        {
            long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            long timeSinceLastChange = currentTime - lastTimeChange;
            long timeLimitSinceLastChange = 1000;

            if (timeSinceLastChange > timeLimitSinceLastChange)
            {
                videoTimeSeconds = seconds;
                lastTimeChange = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                if (GetSuggestedVideoState() == VideoState.Ended)
                {
                    UserManager.SetStateForAll(VideoState.Playing);
                }

                return;

            }
            
        }

        public void StartEmptyRoomCountDown()
        {
            destroyRoomTokenSource = new CancellationTokenSource();

            var task = Task.Run(async () =>
            {
                await Task.Delay(timeUntilEmptyRoomDestroyedMS, destroyRoomTokenSource.Token);
                RoomShouldBeDestroyed = true;
                logger.LogInformation("[VSY] Room " + id + " has been empty for " + timeUntilEmptyRoomDestroyedMS / 1000  + " seconds. Room has been tagged to be destroyed");
            },
            destroyRoomTokenSource.Token);
        }

        public User Join(string name, string sessionID, IPAddress ipAddress)
        {
            User user = UserManager.Join(name, sessionID, ipAddress);
            HandleJoiningUser(user);

            if (destroyRoomTokenSource != null) 
            {
                logger.LogInformation("[VSY] Stopped room destroy count down");
                destroyRoomTokenSource.Cancel();
                RoomShouldBeDestroyed = false;
            }
            

            return user;
        }

        public void Leave(int userId)
        {
            logger.LogInformation("[VSY]User with user id " + userId + " has left.");
            UserManager.RemoveFromUserList(userId);

            if (UserManager.GetNumUsers() <= 0)
            {
                StartEmptyRoomCountDown();
                logger.LogInformation("[VSY] Started room destroy count down");
            }
        }

        private void HandleJoiningUser(User user)
        {
            if (UserManager.GetNumUsers() == 1)
            {
                UserManager.SetStateForUser(user.id, VideoState.Paused);
                    
            }
            else
            {
                UserManager.SetStateForUser(user.id, GetSuggestedVideoState());
            }
        }

        public int GetMinutesSinceRoomCreation()
        {
            long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            long timeSinceCreation = currentTime - roomCreationTime;

            int minutes = (int) (timeSinceCreation / 60000);
            return minutes;
        }

        public void Leave(User user)
        {
            UserManager.RemoveFromUserList(user);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                CancelPeriodicTasks();
                logger.LogDebug("[VSY]Room " + id + " disposed.");
            }

            disposed = true;
        }
    }
}
