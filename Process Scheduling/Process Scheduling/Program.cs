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
            Console.Write("Enter number of processes: ");
            int processes = Convert.ToInt32(Console.ReadLine());
            Thread main = new Thread(processes);
            main.initiateProcesses();

            Console.Write("Enter scheduling: \n 1. SJF\n 2. SRT\n 3. FIFO\n 4. HRRN\nEnter your choice: ");
            int scheduling = Convert.ToInt32(Console.ReadLine());
            string[] timeline;
            switch (scheduling)
            {
                case 1:
                    timeline = main.execute(Schedule.SJF);
                    break;
                case 2:
                    timeline = main.execute(Schedule.SRT);
                    break;
                case 3:
                    timeline = main.execute(Schedule.FIFO);
                    break;
                default:
                    timeline = main.execute(Schedule.HRRN);
                    break;
            }

            Console.Clear();
            foreach (string time in timeline)
                Console.Write(time + (" -> "));
            Console.WriteLine();
            main.generateTable();
            Console.WriteLine("Average Turnaround Time: " + main.avgturnaround);
            
        }
        
    }

    class Thread
    {
        Process[] processes;

        internal Thread(int numberOfProcesses)
        {
            processes = new Process[numberOfProcesses];
        }

        internal float avgturnaround
        {
            get
            {
                int sum = 0;
                foreach (Process process in processes)
                    sum += process.turnaround;
                return (((float) sum) / ((float) processes.Length));
            }
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


        internal string[] execute(Schedule schedule)
        {
            string[] timeline;
            switch (schedule)
            {
                case Schedule.HRRN:
                    timeline = HRRN();
                    break;
                case Schedule.FIFO:
                    timeline = FIFO();
                    break;
                case Schedule.SJF:
                    timeline = SJF();
                    break;
                default:
                    timeline = SRT();
                    break;
            }

            return timeline;
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

        private string[] HRRN()
        {
            List<string> timeline = new List<string>();
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
                            {
                                process.execute(process.execution, timeSlice);
                                timeline.Add(process.id);
                            }

                if (allFinished) break;

            }
            return timeline.ToArray();
        }

        private string[] FIFO()
        {
            List<string> timeline = new List<string>();
            for (int timeSlice = 0; true; timeSlice++)
            {
                Process[] arrived = arrivedProcesses(timeSlice);
                Array.Sort(arrived, delegate(Process process, Process _process)
                {
                    return process.arrival.CompareTo(_process.arrival);
                });

                foreach (Process process in arrived)
                    if (!(process.executingState == ExecutingState.Finished))
                    {
                        process.execute(process.execution, timeSlice);
                        timeSlice += process.execution;
                        timeline.Add(process.id);
                    }

                if (allFinished) break;
            }
            return timeline.ToArray();
        }

        private string[] SJF()
        {
            List<string> timeline = new List<string>();
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
                            timeline.Add(process.id);
                        }

                if (allFinished) break;
            }
            return timeline.ToArray();
        }

        private string[] SRT()
        {
            List<string> timeline = new List<string>();
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
                        {
                            process.execute(1, timeSlice);
                            timeline.Add(process.id);
                        }

                if (allFinished) break;

            }

            return timeline.ToArray();
        }

        internal void generateTable()
        {                    
            foreach (Process process in processes)
                Console.WriteLine(process.id + " " + process.arrival + " " + process.execution + " " + process.wait + " " + process._finish + " " + process.utilization(process._finish));        
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
        internal int _start, _finish;
        internal ExecutingState executingState;
        internal int remainingTime
        {
            get
            {
                return ((execution - completed > 0) ? (execution - completed) : 1000000);
            }
        }

        internal int finish 
        {
            get 
            {
                return _finish;
            }
        }

        internal int turnaround
        {
            get
            {
                return _finish - arrival;
            }
        }

        internal void execute(int timeSlice, int threadTime)
        {
            _start = ((completed == 0) ? threadTime : _start);
            executingState = ExecutingState.Executing;
            completed += timeSlice;
            //TODO: Show Execution
            if (completed < execution) return;
            else
            {
                executingState = ExecutingState.Finished;
                _finish = threadTime;
            }
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

    //class TimeLine
    //{
    //    int height, width;
    //    int x, y;
    //    int blocks;

    //    public TimeLine()
    //    {
    //        this.height = 1;
    //        this.width = 1;
    //        this.blocks = 0;
    //        this.x = 0;
    //        this.y = 0;
    //    }

    //    public void append(ConsoleColor color)
    //    {
    //        x += width + 2;
    //        string s = "╔";
    //        string space = "";
    //        string temp = "";
    //        for (int i = 0; i < width; i++)
    //        {
    //            space += " ";
    //            s += "═";
    //        }

    //        for (int j = 0; j < x; j++)
    //            temp += " ";

    //        s += "╗" + "\n";

    //        for (int i = 0; i < height; i++)
    //            s += temp + "║" + space + "║" + "\n";

    //        s += temp + "╚";
    //        for (int i = 0; i < width; i++)
    //            s += "═";

    //        s += "╝" + "\n";

    //        Console.ForegroundColor = color;
    //        Console.CursorTop = y;
    //        Console.CursorLeft = x;
    //        Console.Write(s);
    //        Console.ResetColor();
    //    }
    //}

    //struct Color
    //{
    //    static Random random = new Random();
    //    public static ConsoleColor randomColor
    //    {
    //        get
    //        {
    //            return colors[random.Next() % colors.Length];
    //        }
    //    }

    //    static ConsoleColor[] colors = { ConsoleColor.Blue, ConsoleColor.Red, ConsoleColor.White, ConsoleColor.Yellow, ConsoleColor.Cyan };

    //}
}
