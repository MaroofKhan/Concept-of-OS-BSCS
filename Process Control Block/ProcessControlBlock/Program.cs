using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessControlBlock
{
    class RandomNumber
    {
        static Random rand = new Random();
        public static int generate (int minimum, int maximum)
        {
            int number = rand.Next(minimum, maximum);
            return number;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter number of Processes: ");
            int numberOfProcesses = Convert.ToInt32(Console.ReadLine());
            Console.Write("Enter 'Quantum' number: ");
            int quantum = Convert.ToInt32(Console.ReadLine());
            ProcessControlBlock PCB = new ProcessControlBlock(numberOfProcesses, quantum);
            PCB.execute();
        }

    }

    class ProcessControlBlock
    {
        public Process[] processes;
        public int quantum;

        public Register programCounter, instructionRegister;

        public ProcessControlBlock(int numberOfProcess, int quantum)
        {
            this.initiateProcesses(numberOfProcess);
            this.quantum = quantum;

            this.programCounter = new Register();
            this.instructionRegister = new Register();

            this.display();
        }

        void next(int numberOfProcess)
        {
            while (true)
            {
                numberOfProcess++;
                if (allFinished) return;
                else if (numberOfProcess < processes.Length)
                    if (processes[numberOfProcess].finished) continue;
                    else
                    {
                        Process process = processes[numberOfProcess];
                        Instruction instruction = process.instruction;

                        if (instruction == null) continue;

                        this.programCounter.update(process, instruction);
                        return;
                    }
                else numberOfProcess = -1;
            }

        }
        
        void display()
        {
            foreach (Process process in processes)
                Console.WriteLine("Process " + process.id + ", contains " + process.instructions.Length + " instructions.");
        }

        public void execute()
        {
            int cycleNumber = 1;
            Console.WriteLine();
            while (!allFinished)
            {
                for (var index = 0; index < processes.Length; index++)
                {
                    Process process = processes[index];

                    if (process.finished) continue;
                    process.execute(quantum);

                    if (programCounter.process == null)
                    {
                        this.instructionRegister.update(processes[0], processes[0].instructions[0]);
                        this.next(0);
                    }
                    else
                    {
                        instructionRegister.update(programCounter.process, programCounter.instruction);
                        next(index);
                    }

                    Console.WriteLine("Quantum Cycle# " + (cycleNumber++));
                    Console.WriteLine("Instruction Register: " + instructionRegister.address);
                    Console.WriteLine("Program Counter: " + ((programCounter.address == instructionRegister.address) ? "Empty (NULL)" : programCounter.address));
                    Console.WriteLine();
                }
            }

            Console.WriteLine("All processes have now finished executing.");
        }

        void initiateProcesses(int numberOfProcesses)
        {
            processes = new Process[numberOfProcesses];
            for (int index = 0; index < numberOfProcesses; index++)
                processes[index] = new Process(index);

        }

        bool allFinished
        {
            get
            {
                foreach (Process process in processes)
                    if (!(process.finished)) return false;
                return true;
            }
        }
    }

    class Register
    {
        public Process process;
        public Instruction instruction;

        public void update(Process process, Instruction instruction)
        {
            this.process = process;
            this.instruction = instruction;
        }

        public string address { get { return (process.id + ", " + instruction.number + "."); } }
    }

    class Process
    {
        public string id;
        public Instruction[] instructions;
        public State state;

        public bool finished { get { return state == State.Finished; } }

        public Process(int number)
        {
            this.id = "P" + number;
            this.state = State.NotFinished;
            int instructions = RandomNumber.generate(4, 40);
            this.initiateInstructions(instructions);
        }

        public int executed
        {
            get
            {
                int executed = 0;
                foreach (Instruction instruction in instructions)
                    if (instruction.executed) executed++;
                return executed;
            }
        }

        public void execute(int numberOfInstructions)
        {
            int instructions = numberOfInstructions;
            for (var index = 0; index < instructions; index++)
                if (allFinished) { this.state = State.Finished; return; }
                else if (this.instructions[index].executed) { instructions++; }
                else { this.instructions[index].execute(); }

            if (executed == this.instructions.Length) this.state = State.Finished;

        }

        public Instruction instruction
        {
            get
            {
                int instructions = this.instructions.Length;
                for (var index = 0; index < instructions; index++)
                    if (this.instructions[index].executed) continue;
                    else return this.instructions[index];
                return null;
            }
        }

        public bool allFinished
        {
            get
            {
                foreach (Instruction instruction in instructions)
                    if (!(instruction.executed)) return false;
                return true;
            }
        }

        void initiateInstructions(int numberOfInstructions)
        {
            instructions = new Instruction[numberOfInstructions];
            for (int index = 0; index < numberOfInstructions; index++)
                instructions[index] = new Instruction(index);
        }
    }

    class Instruction
    {
        public int number;
        public bool executed;
        public Instruction(int number)
        {
            this.number = number;
            this.executed = false;
        }

        public void execute() { this.executed = true; }
    }

    enum State
    {
        Finished,
        NotFinished
    }
}
