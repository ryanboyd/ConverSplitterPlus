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







        private void StartButton_Click(object sender, EventArgs e)
        {

            FolderBrowser.Description = "Please choose the location of your INPUT .txt files that you want to split";

            if (FolderBrowser.ShowDialog() != DialogResult.Cancel) {

                string TextFileFolder = FolderBrowser.SelectedPath.ToString();
                           
                FolderBrowser.Description = "Please choose the OUTPUT location for your files";

                if (FolderBrowser.ShowDialog() != DialogResult.Cancel)
                {

                    string OutputFileLocation = FolderBrowser.SelectedPath.ToString();
                
                    StartButton.Enabled = false;
                    SpeakerListTextBox.Enabled = false;
                    ScanSubfolderCheckbox.Enabled = false;
                    EncodingDropdown.Enabled = false;
                    SpeakersMultipleLinesCheckbox.Enabled = false;
                    DetectSpeakersButton.Enabled = false;
                    BgWorker.RunWorkerAsync(new string[] {TextFileFolder, OutputFileLocation});
                }
            } 

        }








        private void DetectSpeakersButton_Click(object sender, EventArgs e)
        {

            string DelimiterString = ": ";
            string MaxTagLengthString = "20";
            int MaxTagLengthInt = 20;

            //get the speaker delimiter here
            if (ShowInputDialog(ref DelimiterString, "Speaker Tag Delimiter:") == DialogResult.OK)
            {



                if (ShowInputDialog(ref MaxTagLengthString, "Maximum tag length to consider:") == DialogResult.OK)
                {
                    bool isNumeric = int.TryParse(MaxTagLengthString, out MaxTagLengthInt);

                    if (isNumeric)
                    {



                        FolderBrowser.Description = "Please choose the location of your INPUT .txt files. These are the files in which you want to detect all of the speakers.";
                        FolderBrowser.ShowDialog();
                        string TextFileFolder = FolderBrowser.SelectedPath.ToString();

                        if (TextFileFolder != "")
                        {




                            if (DelimiterString != "")
                            {
                                StartButton.Enabled = false;
                                SpeakerListTextBox.Enabled = false;
                                ScanSubfolderCheckbox.Enabled = false;
                                EncodingDropdown.Enabled = false;
                                SpeakersMultipleLinesCheckbox.Enabled = false;
                                DetectSpeakersButton.Enabled = false;
                                DetectSpeakersBGWorker.RunWorkerAsync(new string[] { TextFileFolder, MaxTagLengthString, DelimiterString});
                            }
                        }



                    }
                    else
                    {
                        MessageBox.Show("Your maximum tag length to consider must be a whole number.\r\nThis number will be used to make sure that only relatively short strings\r\nare considered as possibly \"real\" speaker tags.");
                    }

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


                    string outputFolder_Subs = "";

                    if (SearchDepth == SearchOption.AllDirectories)
                    {
                        string subfolder = Path.GetDirectoryName(fileName).Replace(((string[])e.Argument)[0], "");
                        outputFolder_Subs = System.IO.Path.Combine(outputFolder, subfolder.Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                        try
                        {
                            System.IO.Directory.CreateDirectory(outputFolder_Subs);
                        }
                        catch
                        {
                            MessageBox.Show("ConverSplitterPlus could not create a subdirectory in your output folder.\r\nIs your output directory write protected?");
                            e.Cancel = true;
                        }

                    }



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
                    if ((FoundSpeaker == false) && (PreviousSpeaker != ""))
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
                        if (SearchDepth == SearchOption.AllDirectories)
                        {
                            OutputFilename = System.IO.Path.Combine(outputFolder_Subs, OutputFilename);
                        }
                        else
                        {
                            OutputFilename = System.IO.Path.Combine(outputFolder, OutputFilename);
                        }
                        

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
            DetectSpeakersButton.Enabled = true;
            FilenameLabel.Text = "Finished!";
            MessageBox.Show("ConverSplitterPlus has finished analyzing your texts.", "Analysis Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }












        




        private void DetectSpeakersBGWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //set up our sentence boundary detection
            Regex NewlineClean = new Regex(@"[\r\n]+", RegexOptions.Compiled);

            //selects the text encoding based on user selection
            Encoding SelectedEncoding = null;

            this.Invoke((MethodInvoker)delegate ()
            {
                SelectedEncoding = Encoding.GetEncoding(EncodingDropdown.SelectedItem.ToString());

            });




            //make sure that we convert our max length to an integer




            //get the list of files
            var SearchDepth = SearchOption.TopDirectoryOnly;
            if (ScanSubfolderCheckbox.Checked)
            {
                SearchDepth = SearchOption.AllDirectories;
            }
            var files = Directory.EnumerateFiles(((string[])e.Argument)[0], "*.txt", SearchDepth);


            //pull out the arguments and put them into more accessible variable names
            int MaxTagLength = int.Parse(((string[])e.Argument)[1]);
            string DelimiterString = ((string[])e.Argument)[2];
            int DelimiterLength = DelimiterString.Length;

            HashSet<string> SpeakerList = new HashSet<string>();


            try
            {

                foreach (string fileName in files)
                {


                    //set up our variables to report
                    string Filename_Clean = Path.GetFileName(fileName);

                    //report what we're working on
                    FilenameLabel.Invoke((MethodInvoker)delegate
                    {
                        FilenameLabel.Text = "Analyzing: " + Filename_Clean;
                    });


                    //do stuff here
                    string[] readText_Lines = NewlineClean.Split(File.ReadAllText(fileName, SelectedEncoding));
                    int NumberOfLines = readText_Lines.Length;


                    //loop through all of the lines in each text
                    for (int i = 0; i < NumberOfLines; i++)
                    {

                        string CurrentLine = readText_Lines[i].Trim();

                        int IndexOfDelimiter = CurrentLine.IndexOf(DelimiterString);

                        if (IndexOfDelimiter > -1)
                        {
                            string SpeakerTag = CurrentLine.Substring(0, IndexOfDelimiter + DelimiterLength);

                            if ((SpeakerTag.Length <= MaxTagLength) && !SpeakerList.Contains(SpeakerTag))
                            {
                                SpeakerList.Add(SpeakerTag);
                            }

                        }
                        

                        //end of for loop through each line
                    }



                    

                    //end of for loop through each file
                }



                //end of try block
            }
            catch
            {
                MessageBox.Show("ConverSplitterPlus encountered an issue while opening / scanning your files.\r\n?Are your text files open in another program?");
            }

            e.Result = SpeakerList;

        }

        private void DetectSpeakersBGWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            SpeakerListTextBox.Text = string.Join("\r\n", e.Result as HashSet<string>);

            StartButton.Enabled = true;
            SpeakerListTextBox.Enabled = true;
            ScanSubfolderCheckbox.Enabled = true;
            EncodingDropdown.Enabled = true;
            SpeakersMultipleLinesCheckbox.Enabled = true;
            DetectSpeakersButton.Enabled = true;
            FilenameLabel.Text = "Finished!";
            MessageBox.Show("ConverSplitterPlus has finished analyzing your texts.", "Analysis Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }














        //borrowed from here:
        //https://stackoverflow.com/a/17546909
        private static DialogResult ShowInputDialog(ref string input, string DialogName)
        {
            System.Drawing.Size size = new System.Drawing.Size(300, 70);
            Form inputBox = new Form();

            inputBox.StartPosition = FormStartPosition.CenterParent;

            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            inputBox.ClientSize = size;
            inputBox.Text = DialogName;

            System.Windows.Forms.TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox.Location = new System.Drawing.Point(5, 5);
            textBox.Text = input;
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 39);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, 39);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            input = textBox.Text;
            return result;
        }


        








    }
    


}
