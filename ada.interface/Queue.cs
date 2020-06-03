using System;
using System.Collections.Generic;
using System.Text;

namespace ada.interface_
{
    class Queue
    {
        // Dictionary will store all messages pushed to the Queue
        private static Dictionary<string, List<Message>> queue = new Dictionary<string, List<Message>>();

        /// <summary>
        /// Pushes message to queue. if the target does not exist, add new target and push
        /// </summary>
        /// <param name="newMessage"></param>
        public static bool pushMessage(Message newMessage)
        {
            // Debug write message output
            Debugger.Write($"Message target: {newMessage.getTarget()}", 5);
            Debugger.Write($"Message content: {newMessage.getContent()}", 5);
            Debugger.Write($"Message ttc: {newMessage.getTimeToConsume().ToString()}", 5);

            try
            {
                // If target does not exist, add it to dictionary
                if (!queue.ContainsKey(newMessage.getTarget()))
                {
                    Debugger.Write($"Adding new message target: {newMessage.getTarget()}", 5);
                    queue.Add(newMessage.getTarget(), new List<Message>());
                } else { Debugger.Write($"Existing target found: {newMessage.getTarget()}", 5); }

                // Add newMessage to target
                List<Message> newMessageList = queue[newMessage.getTarget()];
                newMessageList.Add(newMessage);
                queue[newMessage.getTarget()] = newMessageList;
                Debugger.Write($"New message was added successfully!", 5);
                return true;
            }
            catch (Exception e)
            {
                Debugger.Write($"Error pusing to queue: {e.ToString()}", 1);
                return false;
            }
        }
    }
}
