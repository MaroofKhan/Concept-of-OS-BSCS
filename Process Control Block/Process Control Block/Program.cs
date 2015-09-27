using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Process_Control_Block
{
    static class Program
    {
        
        static void Main(string[] args)
        {
            Register IR = new Register();
            Register PCB = new Register();
            
            LinkedList<Process> processes = new LinkedList<Process>();
            
            Console.Write("Enter number of processes: ");
            int numberOfProcesses = Convert.ToInt32(Console.ReadLine());
            
            for (int processNumber = 0; processNumber < numberOfProcesses; processNumber++)
            {
                Process process = new Process(processNumber + 1);
                if (processes.First == null)
                    processes.AddFirst(process);
                else
                    processes.AddAfter(processes.Last, process);
                
                Console.WriteLine("Process# {0} contains {1} instructions.", (process.number), (process.instructions));
            }

            Console.Write("Enter 'Quantum' number: ");
            int quantum = Convert.ToInt32(Console.ReadLine());

            LinkedListNode<Process> current = processes.First;

            while ((Process.NotFinished(processes.ToArray())))
            {
                IR.process = current.Value.number;
                IR.instruction = current.Value.done + 1;

                LinkedListNode<Process> process = current.Next ?? processes.First;

                while (process != null && process.Value.state == State.Finished)
                {
                    process = process.Next;
                }

                if (process == null) break;

                PCB.process = process.Value.number;
                PCB.instruction = process.Value.done + 1; 

                if (current.Value.state == State.NotFinished)
                {
                    current.Value.done += quantum;
                    if (current.Value.done >= current.Value.instructions) current.Value.state = State.Finished;
                    Console.WriteLine("**************************************************");
                    Console.WriteLine("Process# " + current.Value.number + " executing...");
                    Console.WriteLine("IR: " + IR.address);
                    Console.WriteLine("PCB: " + PCB.address);

                    if (current.Value.done >= current.Value.instructions) current.Value.state = State.Finished;
                }

                current = current.Next ?? processes.First;
            }

            Console.WriteLine("**************************************************");
            Console.WriteLine("Process# " + current.Value.number + " executing...");
            Console.WriteLine("IR: " + IR.address);
            Console.WriteLine("PCB: NULL (All processes are finished!)");


        }


        public static Random rand = new Random();
        public static int random 
        {
            get {
                int number = rand.Next(1, 6);
                return number;
            }
        }

    }

    enum State
    {
        Finished,
        NotFinished
    }
    class Process
    {
        public int number;
        public int instructions;
        public int done;
        public State state;

        public Process(int number)
        {
            this.number = number;
            instructions = Program.random;
            done = 0;
            state = State.NotFinished;
        }

        public static bool NotFinished(Process[] processes)
        {
            foreach (Process process in processes)
            {
                if (process.state == State.NotFinished) return true;
            }
            return false;
        }

    }

    class Register
    {
        public int process;
        public int instruction;
        public string address
        {
            get
            {
                return "Process# " + process + ", instruction# " + instruction + ".";
            }
        }
    }

}
