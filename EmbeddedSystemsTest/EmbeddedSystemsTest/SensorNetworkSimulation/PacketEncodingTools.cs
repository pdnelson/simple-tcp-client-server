﻿using EmbeddedSystemsTest.SensorNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedSystemsTest.SensorNetworkSimulation
{
    public class PacketEncodingTools
    {
        // This is important for the simulation. This will convert the data arrays we get from CSV files
        // to bytes that we can send to the SensorNetworkServer
        public static byte[] ConvertDataArraysToBytes(RawAccelerometerData[] elAccl, RawAccelerometerData[] azAccl, RawAccelerometerData[] cbAccl, double[] elTemps, double[] azTemps, double[] elEnc, double[] azEnc, SensorStatuses statuses)
        {
            int dataSize = CalcDataSize(elAccl.Length, azAccl.Length, cbAccl.Length, elTemps.Length, azTemps.Length, elEnc.Length, azEnc.Length);

            // If you want to input raw data instead, just comment out the next few loops.
            // They exist so that we can input data into our CSV files that make sense to us, since
            // raw data values are not very readable

            // Convert elevation temperature to raw data
            short[] rawElTemps = new short[elTemps.Length];
            for (int i = 0; i < elTemps.Length; i++)
            {
                rawElTemps[i] = ConvertTempCToRawData(elTemps[i]);
            }

            // Convert azimuth temperature to raw data
            short[] rawAzTemps = new short[azTemps.Length];
            for (int i = 0; i < azTemps.Length; i++)
            {
                rawAzTemps[i] = ConvertTempCToRawData(azTemps[i]);
            }

            // Convert elevation position to raw data
            short[] rawElEnc = new short[elEnc.Length];
            for(int i = 0; i < elEnc.Length; i++)
            {
                rawElEnc[i] = ConvertDegreesToRawElData(elEnc[i]);
            }

            // Convert azimuth position to raw data
            short[] rawAzEnc = new short[azEnc.Length];
            for (int i = 0; i < azEnc.Length; i++)
            {
                rawAzEnc[i] = ConvertDegreesToRawAzData(azEnc[i]);
            }

            bool[] sensorStatusBoolArray = new bool[] {
                statuses.ElevationAccelerometerStatus == SensorStatus.OKAY,
                statuses.AzimuthAccelerometerStatus == SensorStatus.OKAY,
                statuses.CounterbalanceAccelerometerStatus == SensorStatus.OKAY,
                statuses.ElevationTemperature1Status == SensorStatus.OKAY,
                statuses.ElevationTemperature2Status == SensorStatus.OKAY,
                statuses.AzimuthTemperature1Status == SensorStatus.OKAY,
                statuses.AzimuthTemperature2Status == SensorStatus.OKAY,
                statuses.AzimuthAbsoluteEncoderStatus == SensorStatus.OKAY,
            };

            int errors = 0; // TODO: implement conversion

            return EncodeData(dataSize, elAccl, azAccl, cbAccl, rawElTemps, rawAzTemps, rawElEnc, rawAzEnc, sensorStatusBoolArray, errors);
        }

        // This will take each data array and add it to its proper location in the byte array
        public static byte[] EncodeData(int dataSize, RawAccelerometerData[] elAcclData, RawAccelerometerData[] azAcclData, RawAccelerometerData[] cbAcclData, short[] elTemp, short[] azTemp, short[] elEnc, short[] azEnc, bool[] statuses, int errors)
        {
            byte[] data = new byte[dataSize];

            int i = 0;
            data[i++] = SensorConversionConstants.DataTransmitId;

            // Store the total data size in 4 bytes
            Add32BitValueToByteArray(ref data, ref i, dataSize);

            // Store the sensor statuses
            data[i++] = ConvertBoolArrayToByte(statuses);

            // Store the sensor errors in 3 bytes
            Add24BitValueToByteArray(ref data, ref i, errors);

            // Store elevation accelerometer size in 2 bytes
            Add16BitValueToByteArray(ref data, ref i, elAcclData.Length);

            // Store azimuth accelerometer size in 2 bytes
            Add16BitValueToByteArray(ref data, ref i, azAcclData.Length);

            // Store counterbalance accelerometer size in 2 bytes
            Add16BitValueToByteArray(ref data, ref i, cbAcclData.Length);

            // Store elevation temperature size in 2 bytes
            Add16BitValueToByteArray(ref data, ref i, elTemp.Length);

            // Store azimuth temperature size in 2 bytes
            Add16BitValueToByteArray(ref data, ref i, azTemp.Length);

            // Store elevation encoder size in 2 bytes
            Add16BitValueToByteArray(ref data, ref i, elEnc.Length);

            // Store azimuth encoder size in 2 bytes
            Add16BitValueToByteArray(ref data, ref i, azEnc.Length);

            // Store elevation accelerometer data in a variable number of bytes
            // Each axis occupies 2 bytes, making a total of 6 bytes for each accelerometer data
            for (uint j = 0; j < elAcclData.Length; j++)
            {
                Add16BitValueToByteArray(ref data, ref i, elAcclData[j].X);
                Add16BitValueToByteArray(ref data, ref i, elAcclData[j].Y);
                Add16BitValueToByteArray(ref data, ref i, elAcclData[j].Z);
            }

            // Store azimuth accelerometer data in a variable number of bytes
            // Each axis occupies 2 bytes, making a total of 6 bytes for each accelerometer data
            for (uint j = 0; j < azAcclData.Length; j++)
            {
                Add16BitValueToByteArray(ref data, ref i, azAcclData[j].X);
                Add16BitValueToByteArray(ref data, ref i, azAcclData[j].Y);
                Add16BitValueToByteArray(ref data, ref i, azAcclData[j].Z);
            }

            // Store counterbalance accelerometer data in a variable number of bytes
            // Each axis occupies 2 bytes, making a total of 6 bytes for each accelerometer data
            for (uint j = 0; j < cbAcclData.Length; j++)
            {
                Add16BitValueToByteArray(ref data, ref i, cbAcclData[j].X);
                Add16BitValueToByteArray(ref data, ref i, cbAcclData[j].Y);
                Add16BitValueToByteArray(ref data, ref i, cbAcclData[j].Z);
            }

            // Store elevation temperature data in a variable number of bytes
            // Each temperature occupies 2 bytes
            for (uint j = 0; j < elTemp.Length; j++)
            {
                Add16BitValueToByteArray(ref data, ref i, elTemp[j]);
            }

            // Store azimuth temperature data in a variable number of bytes
            // Each temperature occupies 2 bytes
            for (uint j = 0; j < azTemp.Length; j++)
            {
                Add16BitValueToByteArray(ref data, ref i, azTemp[j]);
            }

            // Store elevation encoder data in a variable number of bytes
            // Each position occupies 2 bytes
            for (uint j = 0; j < elEnc.Length; j++)
            {
                Add16BitValueToByteArray(ref data, ref i, elEnc[j]);
            }

            // Store azimuth encoder data in a variable number of bytes
            // Each position occupies 2 bytes
            for (uint j = 0; j < azEnc.Length; j++)
            {
                Add16BitValueToByteArray(ref data, ref i, azEnc[j]);
            }

            return data;
        }

        // Calculates the size of the packet that will be sent to the sensor network
        // This value will be used to create the byte array
        public static int CalcDataSize(int Acc0Size, int Acc1Size, int Acc2Size, int Temp1Size, int Temp2Size, int ElEnSize, int AzEnSize)
        {
            // 1 for the transmit ID
            // 4 for the total data size
            // 4 for the sensor statuses and errors
            // 14 for each sensor's data size (each sensor size is 2 bytes, with 7 sensors total)
            int length = 1 + 4 + 4 + 14;

            // Each accelerometer axis is 2 bytes each. With three axes, that's 6 bytes per accelerometer
            length += Acc0Size * 6;
            length += Acc1Size * 6;
            length += Acc2Size * 6;

            // Each temp and encoder value is 2 bytes
            length += Temp1Size * 2;
            length += Temp2Size * 2;
            length += ElEnSize * 2;
            length += AzEnSize * 2;

            return length;
        }

        // This is so we can give the simulation "real" data, where it will be converted to raw
        // before being encoded. This is approximate, and may not be exact.
        public static short ConvertDegreesToRawElData(double dataToConvert)
        {
            return (short)Math.Round((dataToConvert - 104.375) / -0.25);
        }

        // This is so we can give the simulation "real" data, where it will be converted to raw
        // before being encoded. This is approximate, and may not be exact.
        public static short ConvertDegreesToRawAzData(double dataToConvert)
        {
            return (short)(SensorConversionConstants.AZ_ENCODER_SCALING * dataToConvert / 360 * -1);
        }

        // This converts the degrees from celsius into raw data. This is approximate.
        public static short ConvertTempCToRawData(double dataToConvert)
        {
            return (short)(dataToConvert * 16);
        }

        // A helper function to add 16-bit values to the byte array so we don't have to do this every single time.
        public static void Add16BitValueToByteArray(ref byte[] dataToAddTo, ref int counter, int dataBeingAdded)
        {
            dataToAddTo[counter++] = (byte)((((short)dataBeingAdded) & 0xFF00) >> 8);
            dataToAddTo[counter++] = (byte)((short)dataBeingAdded & 0x00FF);
        }

        // A helper function to add 24-bit values to the byte array so we don't have to do this every single time.
        public static void Add24BitValueToByteArray(ref byte[] dataToAddTo, ref int counter, int dataBeingAdded)
        {
            dataToAddTo[counter++] = (byte)((((short)dataBeingAdded) & 0xFF0000) >> 16);
            dataToAddTo[counter++] = (byte)((((short)dataBeingAdded) & 0x00FF00) >> 8);
            dataToAddTo[counter++] = (byte)((short)dataBeingAdded & 0x0000FF);
        }

        // A helper function to add 32-bit values to the byte array so we don't have to do this every single time.
        public static void Add32BitValueToByteArray(ref byte[] dataToAddTo, ref int counter, int dataBeingAdded)
        {
            dataToAddTo[counter++] = (byte)((((short)dataBeingAdded) & 0xFF000000) >> 24);
            dataToAddTo[counter++] = (byte)((((short)dataBeingAdded) & 0x00FF0000) >> 16);
            dataToAddTo[counter++] = (byte)((((short)dataBeingAdded) & 0x0000FF00) >> 8);
            dataToAddTo[counter++] = (byte)((short)dataBeingAdded & 0x000000FF);
        }

        // This also handles if the numbers are invalid, and will return null if so
        // This should not be needed in the simulation, because we should be getting short arrays from CSVs.
        public static double[] ConvertStringToDoubleArray(string text)
        {
            double[] s;

            try
            {
                s = text.Split(',').Select(double.Parse).ToArray();
            }
            catch
            {
                return null;
            }

            return s;
        }
        
        private static byte ConvertBoolArrayToByte(bool[] source)
        {
            if (source.Length > 8) throw new ArgumentOutOfRangeException("There can only be 8 bits in a byte array.");

            byte result = 0;

            int index = 8 - source.Length;

            // Loop through the array
            foreach (bool b in source)
            {
                // if the element is 'true' set the bit at that position
                if (b)
                    result |= (byte)(1 << (7 - index));

                index++;
            }

            return result;
        }

        // This also handles if the numbers are invalid, and will return null if so.
        // This should not be needed in the simulation, because we should be getting short arrays from CSVs.
        public static RawAccelerometerData[] ConvertStringsToAccelerometerData(string x, string y, string z)
        {
            RawAccelerometerData[] acc;

            try
            {
                var azX = x.Split(',').Select(short.Parse).ToArray();
                var azY = y.Split(',').Select(short.Parse).ToArray();
                var azZ = z.Split(',').Select(short.Parse).ToArray();

                // All the accelerometer axes should have the same length
                if (azX.Length == azY.Length && azX.Length == azZ.Length)
                {
                    acc = new RawAccelerometerData[azX.Length];
                    for (int i = 0; i < azX.Length; i++)
                    {
                        acc[i].X = azX[i];
                        acc[i].Y = azY[i];
                        acc[i].Z = azZ[i];
                    }
                }
                else return null;
            }
            catch
            {
                return null;
            }

            return acc;
        }
    }
}
