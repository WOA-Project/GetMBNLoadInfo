using ELFSharp.ELF;
using ELFSharp.ELF.Segments;

namespace GetMBNLoadInfo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1 || !(File.Exists(args[0]) || Directory.Exists(args[0])))
            {
                Console.WriteLine("Usage: <Path to Qualcomm MBN file>");
            }

            if (File.Exists(args[0]))
            {
                PrintInfoFromFile(args[0]);
            }

            if (Directory.Exists(args[0]))
            {
                foreach (var el in Directory.EnumerateFiles(args[0], "*.*", SearchOption.AllDirectories))
                {
                    PrintInfoFromFile(el);
                }
            }
        }

        static void PrintInfoFromFile(string file)
        {
            uint min = 0;
            uint max = 0;
            try
            {
                IELF image = ELFReader.Load(File.OpenRead(file), true);

                foreach (ISegment segment in image.Segments)
                {
                    if (segment is Segment<uint> intSegment)
                    {
#if DEBUG
                        Console.WriteLine($"Address: 0x{intSegment.Address:X8}");
                        Console.WriteLine($"Physical Address: 0x{intSegment.PhysicalAddress:X8}");
                        Console.WriteLine($"Size: 0x{intSegment.Size:X8}");
                        Console.WriteLine($"Alignment: 0x{intSegment.Alignment:X8}");
                        Console.WriteLine($"Type: {intSegment.Type}");
                        Console.WriteLine($"Flags: {Enum.GetName(typeof(SegmentFlags), intSegment.Flags)}");
                        Console.WriteLine($"Flags: 0x{(uint)intSegment.Flags:X8}");
#endif

                        if (intSegment.PhysicalAddress != 0 && intSegment.Type == SegmentType.Load && ((uint)intSegment.Flags & 0x08000000) == 0x08000000)
                        {
                            uint loadMin = intSegment.PhysicalAddress;
                            uint loadMax = loadMin + intSegment.Size;

#if DEBUG
                            Console.WriteLine($"0x{loadMin:X8} -> 0x{loadMax:X8}");
#endif

                            if (loadMin < min || min == 0)
                            {
                                min = loadMin;
                            }

                            if (loadMax > max || max == 0)
                            {
                                max = loadMax;
                            }
                        }

#if DEBUG
                        Console.WriteLine();
#endif
                    }
                }

                Console.WriteLine(Path.GetFileName(file));
                Console.WriteLine($"0x{min:X8} -> 0x{max:X8} (Size: 0x{max - min:X8})");
            }
            catch (Exception e)
            {
                Console.WriteLine(file);
                Console.WriteLine("EXCEPTION!");
                Console.WriteLine(e);
            }
        }
    }
}