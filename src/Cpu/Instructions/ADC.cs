﻿using Cpu.Instructions;

namespace Cpu.Instructionns
{
    public abstract class ADC_Base_Instruction : Instruction
    {
        public override void Execute(Cpu cpu)
        {
        }

        //Base class that does the common elements for ADC instructions
        //As there are multiple addressing modes 
        protected void DoADCCalc(Cpu cpu, byte mem)
        {
            //Get carry flag
            byte carry = Convert.ToByte(cpu.GetProcessorStatusFlag(StatusFlagName.Carry));

            //Get accumulator
            byte a = cpu.A;

            ushort result = (ushort)(mem + a + carry); //eg 255 + 255 + 1 is the maximum = 511 = a 9 byte number - so we use a 16 bit number to hold it

            //Set Zero flag
            if (result == 0)
            {
                cpu.SetProcessorStatusFlag(true, StatusFlagName.Zero);
            }

            //Decimal Mode - not really planning to implement this to be honest...as the NES doesnt use it
            if (cpu.GetProcessorStatusFlag(StatusFlagName.DecimalMode))
            {
                if (result > 99)
                {
                    cpu.SetProcessorStatusFlag(true, StatusFlagName.Carry);
                }
                else
                {
                    cpu.SetProcessorStatusFlag(false, StatusFlagName.Carry);
                }
            }
            else
            {
                //Set Carry Flag - look at the 9th bit
                bool carryFlagStatus = Convert.ToBoolean(result >> 8);
                cpu.SetProcessorStatusFlag(carryFlagStatus, StatusFlagName.Carry);
            }

            //Mask off the 8 bytes we are interested in and set accumulator result
            byte accumulatorResult = (byte)(result & 0b0000000011111111); //do a binary and 
            cpu.A = accumulatorResult;

            //Cacluate result if the number was signed - to set the overflow tags
            short resultSigned = (sbyte)((sbyte)(mem) + (sbyte)(a) + carry);

            //Overflow flags           
            if (resultSigned > 127 || resultSigned < -128) //todo check this logic is 100% correct?
            {
                cpu.SetProcessorStatusFlag(true, StatusFlagName.Overflow);
            }
            else
            {
                cpu.SetProcessorStatusFlag(false, StatusFlagName.Overflow);
            }
        }
    }

    [InstructionRegister]
    public class ADC_ZP_Instruction : ADC_Base_Instruction
    {
        public ADC_ZP_Instruction() : base()
        {
            this.mnemonic = "ADC";
            this.hexCode = 0x65;
            this.addressingMode = AddressingMode.ZP;
            this.instructionBytes = 2;
            this.machineCycles = 3;
        }

        public override string Description => "adds the value of memory and carry from the previous operation to the value of the accumulator and stores result in the accumulator.";

        public override void Execute(Cpu cpu)
        {
            cpu.IncrementPC();
            byte lb = cpu.addressBus.ReadByte(cpu.PC);
            ushort address = (ushort)(lb);

            //Read from memory
            byte mem = cpu.addressBus.ReadByte(address);

            base.DoADCCalc(cpu, mem);

            cpu.SetTimingControl(machineCycles);
            cpu.IncrementPC();
        }
    }
}