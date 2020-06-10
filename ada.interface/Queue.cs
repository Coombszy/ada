using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Gets next consumable message from the queue of the target device. if none, return null
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Message getNextMessage(string target)
        {
            // Debug write who the target is
            Debugger.Write($"Getting next consumable message for target: {target}", 5);

            // If target could not be found in queue
            if (!queue.ContainsKey(target))
            {
                Debugger.Write($"Entry for {target} could not be found.", 5);
                return null;
            }

            // Return the first consumable message
            for ( int i = 0; i < queue[target].Count; i++)
            {
                if(queue[target][i].isConsumable())
                {
                    Debugger.Write($"Message found!", 5);
                    Message toReturn = queue[target][i];
                    queue[target].RemoveAt(i);
                    return toReturn;
                }
            }

            // Return null if no messages are ready to consume
            Debugger.Write($"No messages to consume for {target}", 5);
            return null;
        }

        /// <summary>
        /// Returns array of all messages for a target. if none, return null
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Message[] getMessages(string target)
        {
            Debugger.Write($"Get all messages for {target}", 5);

            // If target could not be found in queue
            if (!queue.ContainsKey(target))
            {
                Debugger.Write($"Entry for {target} could not be found.", 5);
                return null;
            }

            if (queue[target].Count == 0)
            {
                return null;
            }
            else {
                return queue[target].ToArray();
            }
        }

        /// <summary>
        /// Returns array of all targets in queue. if none, return null
        /// </summary>
        /// <returns></returns>
        public static string[] getTargets()
        {
            Debugger.Write("Getting all targets within the queue", 5);

            if(queue.Keys.Count == 0)
            {
                Debugger.Write("No targets within queue", 5);
                return null;
            }
            else
            {
                return queue.Keys.ToArray();
            }
        }
    }
}
