using System;
using System.Collections.Generic;
using System.Text;

namespace ada.interface_
{
    class Message
    {
        private string messageTarget;
        private string messageContent;
        private DateTime messageTimeToConsume;

        public Message(string target, string content, DateTime ttc)
        {
            this.messageTarget = target;
            this.messageContent = content;
            this.messageTimeToConsume = ttc;
        }

        /// <summary>
        /// Returns true if the message is consumable
        /// </summary>
        /// <returns></returns>
        public bool isConsumable()
        {
            if (messageTimeToConsume.CompareTo(DateTime.Now) < 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns message target
        /// </summary>
        /// <returns></returns>
        public string getTarget()
        {
            return messageTarget;
        }

        /// <summary>
        /// Returns message content
        /// </summary>
        /// <returns></returns>
        public string getContent()
        {
            return messageContent;
        }

        /// <summary>
        /// Returns message time to consume
        /// </summary>
        /// <returns></returns>
        public DateTime getTimeToConsume()
        {
            return messageTimeToConsume;
        }
    }
}
