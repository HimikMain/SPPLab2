using System;
using System.Threading;
using System.Collections.Concurrent;

namespace SPPLab2
{
    public class TaskQueue
    {
        bool working;

        private Thread[] threads;
        public delegate void TaskDelegate();
        private TaskDelegate[] threadTasks;
        private object[] threadLockers;

        private ConcurrentQueue<TaskDelegate> queue;
        private Thread dequeueThread;

        public TaskQueue(int count)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException("count", "Количество потоков должно быть больше нуля");
            }

            working = true;

            threads = new Thread[count];
            threadTasks = new TaskDelegate[count];
            threadLockers = new object[count];

            for (int i = 0; i < count; i++)
            {
                threadLockers[i] = new object();
                threads[i] = new Thread(() =>
                {
                    int index = Convert.ToInt32(Thread.CurrentThread.Name);
                    while (working)
                    {
                        lock (threadLockers[index])
                        {
                            threadTasks[index]?.Invoke();
                            threadTasks[index] = null;
                        }

                        Thread.Sleep(0);
                    }
                });
                threads[i].Name = i.ToString();
                threads[i].Start();
            }

            queue = new ConcurrentQueue<TaskDelegate>();
            dequeueThread = new Thread(DequeueTasks);
            dequeueThread.Start();
        }

        public void EnqueueTask(TaskDelegate task)
        {
            queue.Enqueue(task);
        }

        public void CloseTasks()
        {
            bool noTasks;
            do
            {
                noTasks = queue.Count == 0;
                for (int i = 0; i < threads.Length; i++)
                {
                    noTasks = noTasks && threadTasks[i] == null;
                }
            }
            while (!noTasks);

            working = false;
        }

        private void DequeueTasks()
        {
            while (working)
            {
                for (int i = 0; i < threads.Length; i++)
                {
                    if (threadTasks[i] == null)
                    {
                        lock (threadLockers[i]){
                            while (!queue.TryDequeue(out threadTasks[i]) && working) { }
                        }
                    }
                }
            }
        }
    }
}