﻿using System;
using System.Collections;
using System.IO;
using System.Text;


namespace m3uParser
{
    public class PlaylistToLastFM
    {
        private static string outputFilename { get; set; }

        [STAThreadAttribute]
        static void Main(string[] args)
        {


            Form1 myform = new Form1();
            myform.Text = "Playlist2Last.fm";

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(myform);

            //string filename = null;
            //init(filename);

        }

        public static void init(string filename)
        {

            // args contains abs. filename of .m3u file

            FileStream fileStream = null;
            SetOutputFilename(filename);
            File.Delete(outputFilename);

            try
            {
                if (filename == null)
                    fileStream = new FileStream("lastfm.m3u", FileMode.Open, FileAccess.Read);
                else
                    fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            }
            catch (Exception)
            {
                Console.WriteLine("Could not open lastfm.m3u");
                Console.WriteLine("Enter filename (without .m3u extension)");
                string file = Console.ReadLine();
                file = file + ".m3u";
                try
                {
                    fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                }
                catch (Exception)
                {
                    Console.Write("Could not find file. Exiting..");
                    Console.Read();
                    Environment.Exit(0);
                }
            }

            // m3u8 files support unicode8
            StreamReader txtReader = new StreamReader(fileStream, Encoding.ASCII);

            //read first (useless) line  // #EXTM3U
            txtReader.ReadLine();

            //M3U FORMAT
            //      #EXTINF:<length (sec)>,<artist> - <title>
            //      <absolute path including filename>    // backslashes (\)

            //      #EXTINF:431,Rush - Working Man
            //      D:\Classic rock\Rush\Rush - The Spirit of Radio - Greatest Hits\01.Working Man.mp3

            // the nth song in the m3u playlist is the (nth line in the file/ 2)
            int num = 0;
            while (txtReader.EndOfStream != true)
            {
                num++;
                // read id3 tags - length, artist, song
                string songInfo = txtReader.ReadLine();
                ArrayList tagList = parseSongInfo(songInfo);

                // don't need this, but may be helpful in future
                string pathInfo = txtReader.ReadLine();
                //    ArrayList pathList = parseFilename(pathInfo);

                // HACK - eek, streamwriter is being created in loop!
                writeOutput(tagList, num);
            }

            txtReader.Close();
        }

        private static void SetOutputFilename(string filename)
        {

            string outputDirectory = Path.GetDirectoryName(filename);
            string outputName = Path.GetFileNameWithoutExtension(filename);
            outputFilename = outputDirectory + "\\" + outputName + ".txt";
        }

        private static void writeOutput(IList tagList, int num)
        {
            //const int LENGTH = 0;
            const int ARTIST = 1;
            const int TITLE = 2;

            StreamWriter writer = new StreamWriter(outputFilename, true);
            string outputLine = num + ". " + "[artist]" + tagList[ARTIST] + "[/artist] - \"[track artist=" + tagList[ARTIST] + "]"
                                + tagList[TITLE] + "[/track]\"";
            writer.WriteLine(outputLine);
            writer.Close();
        }



        public static ArrayList parseSongInfo(string songInfo)
        {


            ArrayList songInfoList = new ArrayList(4);
            try
            {

                // get length           
                songInfo = songInfo.Remove(0, 8); // remove #extinf prefix
                int separatorIndex = songInfo.IndexOf(',');
                string lengthSec = songInfo.Substring(0, separatorIndex);

                // get artist
                songInfo = songInfo.Remove(0, separatorIndex + 1);
                separatorIndex = songInfo.IndexOf(" - ");
                string artist = songInfo.Substring(0, separatorIndex);

                // get title
                songInfo = songInfo.Remove(0, separatorIndex + 3);
                string title = songInfo;

                songInfoList.Add(lengthSec);
                songInfoList.Add(artist);
                songInfoList.Add(title);

            }
            catch (Exception)
            {
                Console.Write("Error");
                songInfoList.Add("Error");
                songInfoList.Add("Error");
                songInfoList.Add("Error");
            }

            return songInfoList;
        }

        private static ArrayList parseFilename(string pathInfo)
        {


            ArrayList pathInfoList = new ArrayList(2);
            string filename = Path.GetFileName(pathInfo);
            string directory = Path.GetDirectoryName(pathInfo);

            pathInfoList.Add(filename);
            pathInfoList.Add(directory);
            return pathInfoList;
        }
    }
}