using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TuringSimulator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly int DELAY = int.Parse(ConfigurationManager.AppSettings["Delay"]);

        private int CounterTape2 = 0;
        private int CounterTape3 = 0;
        private int CounterSteps = 0;

        public MainWindow()
        {

            InitializeComponent();
            UpdateTape();
        }

        #region Initializers
        private void UpdateTape()
        {
            InitializeTape(tape1);
            InitializeTape(tape2);
            InitializeTape(tape3);
        }

        private void InitializeTape(TextBox tape)
        {
            const int kNumberOfSymbols = 31; // Number of symbols to read from each side with respect to the currently-pointed symbol.
            string tapeContent = "";

            tape1.Clear();
            tape2.Clear();
            tape3.Clear();

            for (var i = 0; i < kNumberOfSymbols; i++)
            {
                tapeContent += "_";
            }
            tape1.Text = tapeContent;
            tape2.Text = tapeContent;
            tape3.Text = tapeContent;

        }

        private void InitializeInput(TextBox tape)
        {
            var nr1 = int.Parse(tbNr1.Text);
            var nr2 = int.Parse(tbNr2.Text);


            if (nr1 > 15)
            {
                CounterTape2 = nr1 - 15;
            }

            var nr1AsSymbols = "";
            var nr2AsSymbols = "";

            //Erste Zahl vorbereiten
            for (var i = 0; i < nr1; i++)
            {
                nr1AsSymbols += "0";
            }

            //Zweite Zahl vorbereiten
            for (var i = 0; i < nr2; i++)
            {
                nr2AsSymbols += "0";
            }

            var inputString = nr1AsSymbols + "1" + nr2AsSymbols;

            for (var i = 0; i < inputString.Length; i++)
            {
                //tape1.SelectionStart = 15 + i;
                tape1.Select(15 + i, 1);
                tape1.SelectedText = inputString[i].ToString();
            }

        }
        #endregion

        #region HelperFunctions
        private void IncreaseStepCounter()
        {
            CounterSteps++;
            tbSteps.Text = CounterSteps.ToString();
        }

        private string GetCurrentPositionCharacter(TextBox tb)
        {
            return tb.Text[15].ToString();
        }

        private async Task MoveHeadRight(TextBox tape)
        {
            var text = tape.Text;
            text = text.Substring(1, text.Length - 1);
            text += "_";

            await Task.Delay(DELAY);
            tape.Text = text;
        }

        private async Task MoveHeadLeft(TextBox tape)
        {
            await Task.Delay(DELAY);
            var text = tape.Text;
            text = text.Insert(0, "_");
            //text = text.Substring(0, text.Length - 1);

            await Task.Delay(DELAY);
            tape.Text = text;

            //tape.Select(15, 1);
        }

        private void WriteAtPosition(TextBox tb, string symbol)
        {
            //await Task.Delay(DELAY);

            tb.Select(15, 1);
            var kk = tb.SelectedText;
            tb.SelectedText = symbol;
        }

        private void WriteStepStatus(string state, string tapeNr, string readValue, string writeValue, string direction)
        {
            tbStepOutput.AppendText(state + " Tape " + tapeNr + ": Read: " + readValue + "; Write: " + writeValue + " Move: " + direction + "\r\n");
            tbStepOutput.ScrollToEnd();
        }
        #endregion

        #region Events
        /// <summary>
        /// Button Klick Reset
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            CounterTape3 = 0;
            CounterTape2 = 0;
            CounterSteps = 0;

            tbSteps.Clear();
            tbState.Clear();
            tbStepOutput.Clear();
            tbNr1.Clear();
            tbNr2.Clear();
            tbResult.Clear();

            tbNr1.Focus();

            UpdateTape();
        }

        /// <summary>
        /// Event für Button Klick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            DoLogic();
        }
        #endregion

        #region Multiplication

        /// <summary>
        /// q0
        /// </summary>
        private async Task DoMultiplicationAlgorithm()
        {
            var counter = 0;
            tbState.Text = "q0";


            //Solange erstes Zeichen, erstes Tape ungleich 1 => SONST TRANSITION zu q1
            while (GetCurrentPositionCharacter(tape1) != "1")
            {
                WriteStepStatus("q0", "1", "0", "B", "R");
                WriteStepStatus("q0", "2", "B", "0", "R");
                WriteStepStatus("q0", "3", "B", "B", "N");
                counter++;
                IncreaseStepCounter();
                //Tape 1
                WriteAtPosition(tape1, "_");
                await MoveHeadRight(tape1);

                //Tape 2
                WriteAtPosition(tape2, "0");
                await MoveHeadRight(tape2);
                //await MoveHeadLeft(tape2);

                //Tape 3
                WriteAtPosition(tape3, "_");
            }

            #region Transition von q0 zu q1
            //Tape 1
            IncreaseStepCounter();

            WriteStepStatus("q0", "1", "1", "B", "R");
            WriteStepStatus("q0", "2", "B", "B", "N");
            WriteStepStatus("q0", "3", "B", "B", "N");

            WriteAtPosition(tape1, "_");
            await MoveHeadRight(tape1);

            //Tape 2
            WriteAtPosition(tape2, "_");

            //Tape 3
            WriteAtPosition(tape3, "_");
            #endregion

            await DoSecondStatement();
        }

        /// <summary>
        /// Statement q1 inkl. Transition zu q2
        /// </summary>
        private async Task DoSecondStatement()
        {
            tbState.Text = "q1";


            var t1Char = GetCurrentPositionCharacter(tape1);
            var t2Char = GetCurrentPositionCharacter(tape2);
            var t3Char = GetCurrentPositionCharacter(tape3);

            //TRANSITION zu q4 => Akzeptierender Zustand
            if (t1Char == "_" && t2Char == "_" && t3Char == "_")
            {
                //todo: q4 => Gebe das aktuelle Statement aus in einer StatusBox
                IncreaseStepCounter();
                #region Transition von q1 -> q4
                PrintAcceptedResult();
                #endregion
            }
            else
            {
                #region Transition von q1 -> q2
                IncreaseStepCounter();

                WriteStepStatus("q1", "1", "0", "0", "N");
                WriteStepStatus("q1", "2", "B", "B", "L");
                WriteStepStatus("q1", "3", "B", "B", "N");

                //Tape 1
                WriteAtPosition(tape1, "0");

                //Tape 2
                WriteAtPosition(tape2, "_");
                await MoveHeadLeft(tape2);

                //Tape 3
                WriteAtPosition(tape3, "_");

                #endregion

                DoThirdStatement();
            }

        }

        /// <summary>
        /// Statement q2 inkl. Transition zu q3
        /// </summary>
        private async Task DoThirdStatement()
        {
            tbState.Text = "q2";

            if (CounterTape2 > 0)
            {
                tape2.Text = tape2.Text.Substring(1, tape2.Text.Length - 1);
                for (var i = 0; i < CounterTape2; i++)
                {
                    //IncreaseStepCounter();

                    WriteStepStatus("q2", "1", "1", "B", "R");
                    WriteStepStatus("q2", "2", "B", "B", "N");
                    WriteStepStatus("q2", "3", "B", "B", "N");

                    tape2.Text = tape2.Text.Insert(0, "0");
                    await Task.Delay(DELAY);
                }
            }

            while (GetCurrentPositionCharacter(tape2) == "0")
            {
                IncreaseStepCounter();

                WriteStepStatus("q2", "1", "0", "0", "N");
                WriteStepStatus("q2", "2", "0", "0", "L");
                WriteStepStatus("q2", "3", "B", "B", "N");

                //Tape 1
                WriteAtPosition(tape1, "0");

                //Tape 2
                WriteAtPosition(tape2, "0");
                await MoveHeadLeft(tape2);

                //Tape 3
                WriteAtPosition(tape3, "_");

            }


            #region Transition q2 -> q3
            IncreaseStepCounter();

            WriteStepStatus("q2", "1", "0", "0", "N");
            WriteStepStatus("q2", "2", "B", "B", "R");
            WriteStepStatus("q2", "3", "B", "B", "N");

            WriteAtPosition(tape1, "0");

            WriteAtPosition(tape2, "_");
            await MoveHeadRight(tape2);

            WriteAtPosition(tape3, "_");

            DoFourthStatement();
            #endregion
        }

        /// <summary>
        /// Statement q3
        /// </summary>
        /// <returns></returns>
        private async Task DoFourthStatement()
        {
            tbState.Text = "q3";
            while (GetCurrentPositionCharacter(tape2) == "0")
            {
                IncreaseStepCounter();

                WriteStepStatus("q3", "1", "0", "0", "N");
                WriteStepStatus("q3", "2", "0", "0", "R");
                WriteStepStatus("q3", "3", "B", "0", "R");

                //Tape 1
                WriteAtPosition(tape1, "0");

                //Tape 2
                WriteAtPosition(tape2, "0");
                await MoveHeadRight(tape2);

                //Tape 3
                //todo: Setzte Counter für Tape 3, um die 0 zu zählen. Ihr Count ergibt das Endergebnis
                WriteAtPosition(tape3, "0");
                CounterTape3++;

                await MoveHeadRight(tape3);
            }

            #region Transition q3 -> q1
            IncreaseStepCounter();

            WriteStepStatus("q3", "1", "0", "B", "R");
            WriteStepStatus("q3", "2", "B", "B", "N");
            WriteStepStatus("q3", "3", "B", "B", "N");

            //Tape 1
            WriteAtPosition(tape1, "_");
            await MoveHeadRight(tape1);

            //Tape 2
            WriteAtPosition(tape2, "_");

            //Tape 3
            WriteAtPosition(tape3, "_");

            //Setzte q1 fort
            DoSecondStatement();

            #endregion
        }

        /// <summary>
        /// Akzeptierender Zustand, Ausgabe des Resultats
        /// </summary>
        private void PrintAcceptedResult()
        {
            tbState.Text = "q4";

            tbStepOutput.AppendText("q4 -> Akzeptierender Zustand");

            tbResult.Text = CounterTape3.ToString();

            //MessageBox.Show("Das Resultat lautet: " + CounterTape3);
        }
        #endregion

        private async Task DoLogic()
        {
            InitializeInput(tape1);

            await DoMultiplicationAlgorithm();
        }
    }
}
