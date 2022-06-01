using System;
using System.IO;
using System.Text;

namespace p4tools
{
    public class InMemoryTextFile : TextWriter
    {
        private readonly string m_filename;
        private readonly bool m_deleteEmpty;
        private readonly StringBuilder m_string = new();

        public override Encoding Encoding => Encoding.UTF8;

        public InMemoryTextFile(string filename, bool deleteEmpty = false)
        {
            m_filename = filename;
            m_deleteEmpty = deleteEmpty;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var output = m_string.ToString();
                if (File.Exists(m_filename))
                {
                    if (m_deleteEmpty && string.IsNullOrEmpty(output))
                    {
                        Console.WriteLine($"DELETING {Path.GetFileName(m_filename)}");
                        Perforce.Delete(m_filename);
                    }
                    else
                    {
                        var input = File.ReadAllText(m_filename);
                        if (!string.Equals(input, output, StringComparison.Ordinal))
                        {
                            Console.WriteLine($"UPDATING {Path.GetFileName(m_filename)}");
                            Perforce.Checkout(m_filename);
                            File.WriteAllText(m_filename, m_string.ToString());
                        }
                    }
                }
                else
                {
                    if (m_deleteEmpty && string.IsNullOrEmpty(output))
                    {
                    }
                    else
                    {
                        Console.WriteLine($"CREATING {Path.GetFileName(m_filename)}");
                        Directory.CreateDirectory(Path.GetDirectoryName(m_filename)!);
                        File.WriteAllText(m_filename, output);
                        Perforce.Add(m_filename);
                    }
                }
            }
            base.Dispose(disposing);
        }

        public override void Write(char value)
        {
            m_string.Append(value);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            m_string.Append(buffer, index, count);
        }

        public override void Write(string? value)
        {
            if (value != null)
            {
                m_string.Append(value);
            }
        }

        public override string ToString()
        {
            return m_string.ToString();
        }
    }
}
