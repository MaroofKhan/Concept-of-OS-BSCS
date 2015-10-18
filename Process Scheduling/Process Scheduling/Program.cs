using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Process_Scheduling
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread main = new Thread(3);
            main.initiateProcesses();
            main.execute(Schedule.HRRN);
        }

    }

    class Thread
    {
        Process[] processes;

        internal Thread(int numberOfProcesses)
        {
            processes = new Process[numberOfProcesses];
        }

        internal void initiateProcesses ()
        {
            int index = 0;
            while (++index <= processes.Length)
            {
                Console.WriteLine("Process# {0}: ", index);
                Console.Write("Arrival Time: ");
                int arrival = Convert.ToInt32(Console.ReadLine());
                Console.Write("Execution Time: ");
                int execution = Convert.ToInt32(Console.ReadLine());
                processes[index - 1] = new Process(("P" + index), arrival, execution);
            }
        }


        internal void execute(Schedule schedule)
        {
            switch (schedule)
            {
                case Schedule.HRRN:
                    HRRN();
                    break;
                case Schedule.FIFO:
                    FIFO();
                    break;
                case Schedule.SJF:
                    SJF();
                    break;
                default:
                    SRT();
                    break;
            }
        }

        private Process[] arrivedProcesses(int timeSlice)
        {
            List<Process> executable = new List<Process>();
            Process[] arrived;
            foreach (Process process in processes)
            {
                if (process.arrivalState(timeSlice) == ArrivalState.Arrived)
                    executable.Add(process);
            }

            arrived = executable.ToArray();
            return arrived;
        }

        private bool allFinished
        {
            get
            {
                bool allFinished = true;
                foreach (Process process in processes)
                    if (!(process.executingState == ExecutingState.Finished))
                        allFinished = false;
                return allFinished;
            }
        }

        private void HRRN()
        {
            for (int timeSlice = 0; true; timeSlice++)
            {
                Process[] arrived = arrivedProcesses(timeSlice);
                Array.Sort(arrived, delegate(Process process, Process _process)
                {
                    return process.priority.CompareTo(_process.priority);
                });

                if (arrived.Length == 0) continue;

                foreach (Process process in processes)
                    if (process == arrived.First())
                        if (!(process.executingState == ExecutingState.Finished))
                            process.execute(process.execution, timeSlice);

                if (allFinished) break;

            }
        }

        private void FIFO()
        {
            for (int timeSlice = 0; true; timeSlice++)
            {
                Process[] arrived = arrivedProcesses(timeSlice);
                Array.Sort(arrived, delegate(Process process, Process _process)
                {
                    return process.arrival.CompareTo(_process.arrival);
                });

                foreach (Process process in arrived)
                    if (!(process.executingState == ExecutingState.Finished))
                        process.execute(process.execution, timeSlice);

                if (allFinished) break;
            }
        }

        private void SJF()
        {
            for (int timeSlice = 0; true; timeSlice++)
            {
                Process[] arrived = arrivedProcesses(timeSlice);
                Array.Sort(arrived, delegate(Process process, Process _process)
                {
                    return process.execution.CompareTo(_process.execution);
                });

                if (arrived.Length == 0) continue;
                
                Process infocus = null;
                foreach (Process process in arrived)
                    if (!(process.executingState == ExecutingState.Finished))
                    {
                        infocus = process;
                        break;
                    }
                foreach (Process process in processes)
                    if (process == infocus)
                        if (!(process.executingState == ExecutingState.Finished))
                        {
                            process.execute(process.execution, timeSlice);
                            timeSlice += process.execution;
                        }

                if (allFinished) break;
            }
        }

        private void SRT()
        {
            for (int timeSlice = 0; true; timeSlice++)
            {
                Process[] arrived = arrivedProcesses(timeSlice);
                Array.Sort(arrived, delegate(Process process, Process _process) {
                    return process.remainingTime.CompareTo(_process.remainingTime);
                });

                if (arrived.Length == 0) continue;

                Process infocus = arrived.First();
                foreach (Process process in processes)
                    if (process == infocus)
                        if (!(process.executingState == ExecutingState.Finished))
                            process.execute(1, timeSlice);

                if (allFinished) break;
            }
        }
    }

    enum Schedule
    {
        FIFO, SJF, SRT, HRRN
    }

    enum ArrivalState {
        Arrived,
        Waiting
    }

    enum ExecutingState {
        Waiting,
        Executing,
        Finished
    }

    class Process: IComparable
    {
        internal string id;
        internal int arrival, execution, completed;
        internal int _start;
        internal ExecutingState executingState;

        internal int remainingTime
        {
            get
            {
                return ((execution - completed > 0) ? (execution - completed) : 1000000);
            }
        }

        internal void execute(int timeSlice, int threadTime)
        {
            _start = ((completed == 0) ? threadTime : _start);
            executingState = ExecutingState.Executing;
            completed += timeSlice;
            //TODO: Show Execution
            Console.WriteLine(this.id);
            if (completed < execution) return;
            else executingState = ExecutingState.Finished;
        }

        internal ArrivalState arrivalState(int time)
        {
            return ((time < arrival) ? ArrivalState.Waiting : ArrivalState.Arrived);
        }
        
        internal int wait
        {
            get
            {
                if (executingState == ExecutingState.Waiting) return -1;
                return _start - arrival;
            }
        }

        internal float priority
        {
            get
            {
                if (wait < 0) return -1;
                return (((float)(wait + execution)) / (float)(execution));
            }
        }

        internal int finish(int start)
        {
            return start + execution;
        }

        internal float utilization(int finish)
        {
            return ((float)execution) / ((float)finish);
        }

        internal Process(string id, int arrival, int execution)
        {
            this.id = id;
            this.arrival = arrival;
            this.execution = execution;
            this.completed = 0;
            this.executingState = ExecutingState.Waiting;
        }

        public int CompareTo(object obj)
        {
            if (obj is Process)
            {
                return this.arrival.CompareTo((obj as Process).arrival);
            }

            throw new ArgumentException("DAMN!");
        }
    }

    class TimeLine
    {
        int height, width;
        int x, y;
        int blocks;

        public TimeLine()
        {
            this.height = 1;
            this.width = 1;
            this.blocks = 0;
            this.x = 0;
            this.y = 0;
        }

        public void append(ConsoleColor color)
        {
            x += width + 2;
            string s = "╔";
            string space = "";
            string temp = "";
            for (int i = 0; i < width; i++)
            {
                space += " ";
                s += "═";
            }

            for (int j = 0; j < x; j++)
                temp += " ";

            s += "╗" + "\n";

            for (int i = 0; i < height; i++)
                s += temp + "║" + space + "║" + "\n";

            s += temp + "╚";
            for (int i = 0; i < width; i++)
                s += "═";

            s += "╝" + "\n";

            Console.ForegroundColor = color;
            Console.CursorTop = y;
            Console.CursorLeft = x;
            Console.Write(s);
            Console.ResetColor();
        }
    }

    struct Color
    {
        static Random random = new Random();
        public static ConsoleColor randomColor
        {
            get
            {
                return colors[random.Next() % colors.Length];
            }
        }

        static ConsoleColor[] colors = { ConsoleColor.Blue, ConsoleColor.Red, ConsoleColor.White, ConsoleColor.Yellow, ConsoleColor.Cyan };

    }
}
