﻿using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Video_Syncer.logging;
using Video_Syncer.Models.Playlist;
using Video_Syncer.Models.Users;

namespace Video_Syncer.Models
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
        public Models.Users.IUserManager UserManager { get; set; }

        public double videoTimeSeconds { get; set; }

        private long lastTimeChange = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        private long lastCheck = 0;

        public int periodicTaskMilliseconds = 6000;
        private CancellationTokenSource source;

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
            UserManager = new Models.Users.UserManager(id);
            roomCreationTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            logger = LoggingHandler.CreateLogger<Room>();

            StartPeriodicTasks();
        }

        public Room(IPlaylistManager playlistManager, IUserManager userManager, string id, string name = "")
        {
            this.id = id;
            this.name = name;
            
            currentYoutubeVideoId = "LXb3EKWsInQ";
            currentYoutubeVideoTitle = "";
            videoTimeSeconds = 0;

            roomCreationTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            this.UserManager = userManager;
            this.PlaylistManager = playlistManager;
            logger = LoggingHandler.CreateLogger<Room>();

            StartPeriodicTasks();
        }

        private void StartPeriodicTasks()
        {
            source = new CancellationTokenSource();
            var task = Task.Run(async () => {

                await PeriodicForceLeaveAllTimedOutUsers(source.Token);

            },
            source.Token);
        }

        private void CancelPeriodicTasks()
        {
            logger.LogInformation("[VSY]CancelPeriodicTasks() ran on room " + id);
            source.Cancel();
        }

        public async Task PeriodicForceLeaveAllTimedOutUsers(CancellationToken token)
        {
            while (true)
            {
                if(token.IsCancellationRequested)
                {
                    break;
                }

                new Task(() => UserManager.ForceLeaveAllTimedOutUsers()).Start();
                await Task.Delay(periodicTaskMilliseconds);
            }
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

        public void NewVideoState(int userId, VideoState newState)
        {
            if (newState == VideoState.Paused)
            {
                UserManager.SetStateForAll(VideoState.Paused);
            }
            else if (newState == VideoState.Playing)
            {
                if(GetSuggestedVideoState() == VideoState.Ended)
                {
                    videoTimeSeconds = 0;
                }
                UserManager.SetStateForAll(newState);
                
            }
            else if(newState == VideoState.Ended)
            {
                UserManager.SetStateForUser(userId, VideoState.Ended);

                if(UserManager.AllHasState(VideoState.Ended))
                {
                    UpdateTime();

                    //TODO: Playlist support, play next video.
                    PlaylistObject obj = PlaylistManager.GoToNextVideo();
                    NewVideo(obj);
                }
            }
            UpdateVideoStatistics(videoTimeSeconds, currentYoutubeVideoId);

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

        public void UpdateVideoStatistics(double seconds, string youtubeId)
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

        public User Join(string name, string sessionID)
        {
            User user = UserManager.CreateNewUser(name, sessionID);
            logger.LogInformation("[VSY]Joining user \"" + user.name + "\"");
            Join(user);
            return user;
        }

        public void Leave(int userId)
        {
            logger.LogInformation("[VSY]User with user id " + userId + " has left.");
            UserManager.RemoveFromUserList(userId);
        }

        private void Join(User user)
        {
            if(UserManager.AddToUserList(user))
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
