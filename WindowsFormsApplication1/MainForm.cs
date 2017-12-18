using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;


namespace WindowsFormsApplication1
{

    public partial class Form1 : Form
    {


        //this is what runs at initialization
        public Form1()
        {

            InitializeComponent();
            

            foreach(var encoding in Encoding.GetEncodings())
            {
                EncodingDropdown.Items.Add(encoding.Name);
            }
            EncodingDropdown.SelectedIndex = EncodingDropdown.FindStringExact(Encoding.Default.BodyName);


        }







        private void button1_Click(object sender, EventArgs e)
        {

            FolderBrowser.Description = "Please choose the location of your INPUT .txt files that you want to split";
            FolderBrowser.ShowDialog();
            string TextFileFolder = FolderBrowser.SelectedPath.ToString();

            if (TextFileFolder != "")
            {

                FolderBrowser.Description = "Please choose the OUTPUT location for your files";

                FolderBrowser.ShowDialog();
                string OutputFileLocation = FolderBrowser.SelectedPath.ToString();


                if (OutputFileLocation != "") { 
                    StartButton.Enabled = false;
                    SpeakerListTextBox.Enabled = false;
                    ScanSubfolderCheckbox.Enabled = false;
                    EncodingDropdown.Enabled = false;
                    SpeakersMultipleLinesCheckbox.Enabled = false;
                    BgWorker.RunWorkerAsync(new string[] {TextFileFolder, OutputFileLocation});
                }
            } 

        }

        




        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            
            //set up our sentence boundary detection
            Regex NewlineClean = new Regex(@"[\r\n]+", RegexOptions.Compiled);

            //selects the text encoding based on user selection
            Encoding SelectedEncoding = null;
            bool SpeakerMultipleLines = false;

            this.Invoke((MethodInvoker)delegate ()
            {
                SelectedEncoding = Encoding.GetEncoding(EncodingDropdown.SelectedItem.ToString());
                SpeakerMultipleLines = SpeakersMultipleLinesCheckbox.Checked;

            });




            //the very first thing that we want to do is set up our speaker list

            string[] SpeakerList = NewlineClean.Split(SpeakerListTextBox.Text);

            //if we want things to be case-insensitive, this is what we'd do:
            //string[] SpeakerListList = NewlineClean.Split(SpeakerListTextBox.Text.ToLower());

            //remove blanks
            SpeakerList = SpeakerList.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();



            int SpeakerListLength = SpeakerList.Length;


            //get the list of files
            var SearchDepth = SearchOption.TopDirectoryOnly;
            if (ScanSubfolderCheckbox.Checked)
            {
                SearchDepth = SearchOption.AllDirectories;
            }
            var files = Directory.EnumerateFiles( ((string[])e.Argument)[0], "*.txt", SearchDepth);

            string outputFolder = System.IO.Path.Combine(((string[])e.Argument)[1], "ConverSplitter_Output");

            try
            {
                System.IO.Directory.CreateDirectory(outputFolder);
            }
            catch
            {
                MessageBox.Show("ConverSplitterPlus could not create your output folder.\r\nIs your output directory write protected?");
                e.Cancel = true;
            }



            try { 
            
               foreach (string fileName in files)
                {



                    //set up our variables to report
                    string Filename_Clean = Path.GetFileName(fileName);

                    Dictionary<string, string> Text_Split = new Dictionary<string, string>();


                    //report what we're working on
                    FilenameLabel.Invoke((MethodInvoker)delegate
                    {
                        FilenameLabel.Text = "Analyzing: " + Filename_Clean;
                    });


                    //do stuff here
                    string[] readText_Lines = NewlineClean.Split(File.ReadAllText(fileName, SelectedEncoding));

                    int NumberOfLines = readText_Lines.Length;


                    //loop through all of the lines in each text

                    string PreviousSpeaker = "";
                    
                    for (int i = 0; i < NumberOfLines; i++){

                            string CurrentLine = readText_Lines[i].Trim();

                            //if the line is empty, move along... move along
                            if (CurrentLine.Length == 0)
                            {
                                continue;
                            }

                            bool FoundSpeaker = false;

                            //loop through each speaker in list to see if the line starts with their name
                            for (int j =0; j < SpeakerListLength; j++)
                            {

                                // here's what we do if we find a match
                                if (CurrentLine.StartsWith(SpeakerList[j]))
                                {

                                    FoundSpeaker = true;
                                    PreviousSpeaker = SpeakerList[j];

                                    //clean up the line to remove the speaker tag from the beginning
                                    int Place = CurrentLine.IndexOf(SpeakerList[j]);
                                    CurrentLine = CurrentLine.Remove(Place, SpeakerList[j].Length).Insert(Place, "").Trim() + "\r\n";

                                    if (Text_Split.ContainsKey(SpeakerList[j])){
                                       
                                        Text_Split[SpeakerList[j]] += CurrentLine;
                                    }

                                    else
                                    {
                                        Text_Split.Add(SpeakerList[j], CurrentLine);
                                    }

                                    //break to the next line in the text
                                    break;
                                }


                            }


                            //what we will do if no speaker was found
                            if (FoundSpeaker == false)
                            {
                                if (SpeakerMultipleLines)
                                {
                                    Text_Split[PreviousSpeaker] += CurrentLine.Trim() + "\r\n";
                                }
                            }

                    //end of for loop through each line
                    }



                    //here's where we want to write the output! hooray!
                    foreach (KeyValuePair<string, string> entry in Text_Split)
                    {

                        string OutputFilename = Path.GetFileNameWithoutExtension(fileName) + ";" + entry.Key + ".txt";
                        
                        //clean up broken filenames
                        foreach (var c in Path.GetInvalidFileNameChars()) { OutputFilename = OutputFilename.Replace(c, '_'); }

                        //set the full path of our output
                        OutputFilename = System.IO.Path.Combine(outputFolder, OutputFilename);

                        // write the output
                        using (StreamWriter outputFile = new StreamWriter(new FileStream(OutputFilename, FileMode.Create, FileAccess.Write), SelectedEncoding))
                        {
                            outputFile.Write(entry.Value);
                        }
                    }


                    //end of for loop through each file
                }


          
            //end of try block
            }
            catch
            {
                MessageBox.Show("ConverSplitterPlus could not open your output file\r\nfor writing. Is the file open in another application?");
            }



            
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StartButton.Enabled = true;
            SpeakerListTextBox.Enabled = true;
            ScanSubfolderCheckbox.Enabled = true;
            EncodingDropdown.Enabled = true;
            SpeakersMultipleLinesCheckbox.Enabled = true;
            FilenameLabel.Text = "Finished!";
            MessageBox.Show("ConverSplitterPlus has finished analyzing your texts.", "Analysis Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

       

    }
    


}
