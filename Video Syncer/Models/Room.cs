using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Video_Syncer.Models
{
    public class Room
    {
        public string id { get; set; }
        public string name { get; set; }

        public string currentYoutubeVideoId { get; set; }
        public List<string> youtubePlaylist { get; set; }

        public double videoTimeSeconds { get; set; }

        private int maxUsers = 500;

        public List<User> userList = new List<User>();

        private long lastTimeChange = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        private long lastCheck = 0;

        public int usernameCharacterLimit = 25;

        public Room(string id, string name = "")
        {
            this.id = id;
            this.name = name;
            
            currentYoutubeVideoId = "LXb3EKWsInQ";
            videoTimeSeconds = 0;
            
        }

        public bool isFull()
        {
            if(userList.Count >= maxUsers)
            {
                return true;
            }
            return false;
        }

        public void NewVideo(string youtubeId)
        {
            currentYoutubeVideoId = youtubeId;
            SetStateForAll(VideoState.Playing);
            videoTimeSeconds = 0;
        }

        public VideoState GetSuggestedVideoState()
        {
            if (userList.Count == 0)
            {
                return VideoState.Unstarted;
            }
            else
            {
                return userList.First().videoState;
            }
        }
        public void SetStateForUser(int userId, VideoState state)
        {
            foreach (User user in userList)
            {
                if (userId == user.id)
                {
                    user.videoState = state;
                }
            }
        }

        public void SetStateForAll(VideoState state)
        {
            foreach (User user in userList)
            {
                user.videoState = state;
            }

            /*System.Diagnostics.Debug.WriteLine("Set state for all. All user states are: ");
            foreach (User user in userList)
            {
                System.Diagnostics.Debug.WriteLine(user.name + ": " + user.videoState);
            }*/
        }

        public VideoState GetStateForUser(int userId)
        {
            foreach(User user in userList)
            { 
                if(userId == user.id)
                {
                    return user.videoState;
                }
            }
            return VideoState.Unstarted;
        }

        public Boolean UserIsBuffering()
        {
            foreach(User user in userList)
            {
                if(user.videoState == VideoState.Buffering)
                {
                    return true;
                }
            }
            return false;
        }

        public User GetBufferingUser()
        {
            foreach(User user in userList)
            {
                if(user.videoState == VideoState.Buffering)
                {
                    return user;
                }
            }
            return null;
        }

        public void NewVideoState(int userId, VideoState newState)
        {
            if (newState == VideoState.Paused)
            {
                SetStateForAll(VideoState.Paused);
            }
            else if (newState == VideoState.Playing)
            {
                if(GetSuggestedVideoState() == VideoState.Ended)
                {
                    videoTimeSeconds = 0;
                }
                SetStateForAll(newState);
                
            }
            else if(newState == VideoState.Ended)
            {
                SetStateForAll(VideoState.Ended);

                //TODO: Playlist support, play next video.
                
            }
            UpdateVideoStatistics(videoTimeSeconds, currentYoutubeVideoId);
            
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

        public void UpdateVideoStatistics(double seconds, string youtubeId)
        {
            long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (GetSuggestedVideoState() == VideoState.Playing)
            {
                // Keep the server's interpretation of the video's time up to date with
                // the passing of time. This is so the server doesn't lag behind the client
                // and their playing video.

                if(lastCheck == 0)
                {
                    lastCheck = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    return;
                }

                long timeSinceLastCheck = currentTime - lastCheck;
                double timeSinceLastCheckD = Convert.ToDouble(timeSinceLastCheck);
                double timeToAdd = timeSinceLastCheckD / 1000;
                videoTimeSeconds += timeToAdd;
                System.Diagnostics.Debug.WriteLine("Added " + timeToAdd + " to video time for a total of " + videoTimeSeconds);
                lastCheck = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            }
            else
            {
                lastCheck = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                System.Diagnostics.Debug.WriteLine("Video is not playing. Video is " + GetSuggestedVideoState());
            }

            long timeLimitSinceLastChange = 5000;
            
            long timeSinceLastChange = currentTime - lastTimeChange;

            if (timeSinceLastChange > timeLimitSinceLastChange)
            {
                int gracePeriod = 1;
                double timeDifference = seconds - videoTimeSeconds;
            
                /*if (timeDifference > gracePeriod || timeDifference < (gracePeriod * -1))
                {
                    videoTimeSeconds = seconds;
                    lastChange = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    return;
                }*/
                
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
                    SetStateForAll(VideoState.Playing);
                }

                return;

            }
            
        }

        public User UpdateUser(int userId, double seconds)
        {
            User user = GetUserById(userId);

            if(user == null)
            {
                return null;
            }

            user.videoTimeSeconds = seconds;
            return user;
        }

        public User Join(string name)
        {
            User user = CreateNewUser(name);
            Join(user);
            return user;
        }

        private User CreateNewUser(string name)
        {
            int userId = CreateUniqueUserId();

            if(name.Length > usernameCharacterLimit)
            {
                name = name.Substring(0, usernameCharacterLimit);
            }
            User user = new User(userId, name);
            return user;
        }

        private int CreateUniqueUserId()
        {
            Random random = new Random();
            int id = -1;
            do
            {
                id = random.Next(0, 10000);
            }
            while (UserIdExists(id));

            return id;
        }

        public void Leave(int userId)
        {
            if(UserIdExists(userId))
            {
                User user = GetUserById(userId);
                userList.Remove(user);
            }
        }

        private void Join(User user)
        {
            if(!UserIdExists(user.id) && !isFull())
            {
                userList.Add(user);

                if(userList.Count == 1)
                {
                    SetStateForUser(user.id, VideoState.Paused);
                }
                else
                {
                    SetStateForUser(user.id, GetSuggestedVideoState());
                }
                
            }
        }

        public void Leave(User user)
        {
            if(UserIdExists(user.id))
            {
                userList.Remove(user);
            }
        }

        private Boolean UserIdExists(int id)
        {
            foreach (User user in userList)
            {
                if (user.id == id)
                {
                    return true;
                }
            }
            return false;
        }

        private User GetUserById(int id)
        {
            foreach(User user in userList)
            {
                if(user.id == id)
                {
                    return user;
                }
            }
            return null;
        }
    }
}
