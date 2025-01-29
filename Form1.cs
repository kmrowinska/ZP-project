using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace Projekt_ZP
{
    public partial class Form1 : Form
    {
        private List<int> P, D;
        private Random random;
        private CancellationTokenSource cancellationTokenSource;

        public Form1()
        {
            InitializeComponent();
            P = new List<int>();
            D = new List<int>();
            random = new Random();
        }

        
        //zapis do pliku
        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog sFD = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt",
                Title = "Zapisz instancje do pliku .txt"
            };

            if (sFD.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(sFD.FileName, string.Join(" ", D));
                    MessageBox.Show("Dane zapisano pomyślnie!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas zapisu: {ex.Message}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        //odczyt z pliku
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt",
                Title = "Otwórz plik .txt"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string fileContent = File.ReadAllText(openFileDialog.FileName);
                    richTextBox2.Text = fileContent;

                    D = fileContent.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(int.Parse)
                                    .ToList();

                    
                    numericUpDown1.Enabled = false;
                    numericUpDown2.Enabled = false;
                    numericUpDown3.Enabled = false; 
                    numericUpDown4.Enabled = false;

                    MessageBox.Show("Plik został pomyślnie wczytany!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Wystąpił błąd podczas wczytywania pliku: " + ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        //generowanie instancji
        private void button2_Click_1(object sender, EventArgs e)
        {
            int len = (int)numericUpDown1.Value; // Długość cząsteczki
            int count = (int)numericUpDown4.Value; // Liczba elementów w P
            int insertion = (int)numericUpDown3.Value; // Liczba wstawek
            int deletion = (int)numericUpDown2.Value; // Liczba usunięć

            if (count > len + 1)
            {
                MessageBox.Show("Liczba elementów P (m) nie może być większa niż długość cząsteczki + 1!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Generowanie P i D
            P = Generate_P(len, count);
            D.Clear();
            Generate_D(P, D);
            D.Sort();

            richTextBox1.Text = string.Join(" ", P);
            richTextBox2.Text = string.Join(" ", D);


            if (insertion > 0 || deletion > 0)
            {
                List<int> modifiedD = new List<int>(D); 
                Errors_in_D(modifiedD, insertion, deletion, len, count);
                richTextBox3.Text = string.Join(" ", modifiedD); 
            }
            else
            {
                richTextBox3.Text = string.Join(" ", D); 
            }
        }


        private List<int> Generate_P(int len, int count)
        {
            HashSet<int> P = new HashSet<int> { 0, len};

            Random random = new Random();
            while(P.Count < count)
            {
                P.Add(random.Next(1, len));
            }

            return P.OrderBy(x => x).ToList();

        }


        private void Generate_D(List<int> P, List<int> D)
         {

             HashSet<int> multiset  = new HashSet<int>();    

             for(int i=0; i < P.Count; i++)
             {
                 for(int j = i +1; j <P.Count; j++)
                 {
                     int difference = Math.Abs(P[i] - P[j]);
                     multiset.Add(difference);
                 }
             }

             D.AddRange(multiset);
         }


        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (D != null && D.Any()) 
            {
                int insertion = (int)numericUpDown3.Value;
                int deletion = (int)numericUpDown2.Value;

                if (insertion > 0 || deletion > 0)
                {
                    List<int> modifiedD = new List<int>(D);
                    Errors_in_D(modifiedD, insertion, deletion, (int)numericUpDown1.Value, (int)numericUpDown4.Value);

                    richTextBox3.Text = string.Join(" ", modifiedD);
                }
            }
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown3_ValueChanged(sender, e); 
        }


        //reset danych
        private void button4_Click(object sender, EventArgs e)
        {
            P.Clear();
            D.Clear();
            richTextBox1.Clear();
            richTextBox2.Clear();
            richTextBox3.Clear();

        }

        private void Errors_in_D(List<int> D, int insertion, int deletions, int len, int count)
        {
            Console.WriteLine("D przed zmianami: " + string.Join(" ", D));

            if (insertion <= 0 && deletions <= 0)
            {
                return;
            }


            if (insertion > D.Count)
            {
                MessageBox.Show("Liczba substytucji nie może być większa od liczności multizbioru!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; 
            }

            for(int i=0; i < insertion; i++)
            {
                if(D.Count > 0)
                {
                    int ind = random.Next(D.Count);
                    int new_val = random.Next(1, len);
                    D[ind] = new_val;
                }
            }


            if(deletions > D.Count)
            {
                MessageBox.Show("Liczba delecji nie może być większa od liczności multizbioru!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            for(int i=0; i<deletions; i++)
            {
                int ind = random.Next(D.Count);
                D.RemoveAt(ind);
            }

            Console.WriteLine("D po zmianach: " + string.Join(" ", D));
        }

        private void button9_Click(object sender, EventArgs e)
        {
            cancellationTokenSource?.Cancel();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Run_GA();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            if (D == null || D.Count == 0)
            {
                MessageBox.Show("Najpierw wczytaj lub wygeneruj multizbiór D.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            
            if (listView1.Items.Count == 0)
            {
                MessageBox.Show("Najpierw uruchom algorytm genetyczny, aby uzyskać rozwiązanie.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            
            string zawartoscPliku = $"P: {(P != null && P.Count > 0 ? string.Join(" ", P) : "BRAK DANYCH")}\n";
            zawartoscPliku += $"D (oryginalne): {string.Join(" ", D)}\n";
            zawartoscPliku += $"D (użyte w algorytmie): {richTextBox3.Text}\n"; 
            zawartoscPliku += $"Rozwiązanie: {listView1.Items[0].Text}";

            
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Pliki tekstowe (*.txt)|*.txt",
                Title = "Zapisz wyniki do pliku"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(saveDialog.FileName, zawartoscPliku);
                    MessageBox.Show("Plik został zapisany pomyślnie!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas zapisywania pliku: {ex.Message}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {

        }

        private async void Run_GA()
        {
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;


            int populationSize = (int)numericUpDown5.Value;
            double mutationRate = (double)numericUpDown6.Value / 100;
            double crossoverRate = (double)numericUpDown7.Value / 100;
            int generations = (int)numericUpDown8.Value;
            int tournamentSize = (int)numericUpDown9.Value;
            int maxGenerations = (int)numericUpDown10.Value;

            listView4.Items.Clear();
            listView1.Items.Clear();

            
            listView4.View = View.Details;
            listView4.Columns.Clear();
            listView4.Columns.Add("Generacja ", 150);
            listView4.Columns.Add("Rozwiązanie", 250);

            listView1.View = View.Details;
            listView1.Columns.Clear();
            listView1.Columns.Add("Najlepsze globalnie", 300);

            try
            {
                var progress = new Progress<(int Progress, List<int> CurrentBest, List<int> OverallBest)>(report =>
                {
                    toolStripProgressBar1.Value = report.Progress; 

                
                    var generationItem = new ListViewItem($"Generacja {listView4.Items.Count + 1}");
                    generationItem.SubItems.Add(string.Join(" ", report.CurrentBest.OrderBy(x => x)));
                    listView4.Items.Add(generationItem);
                    listView1.Items.Clear();
                    var overallItem = new ListViewItem(string.Join(" ", report.OverallBest.OrderBy(x => x)));
                    listView1.Items.Add(overallItem);
                    listView4.EnsureVisible(listView4.Items.Count - 1);
                });

                await Task.Run(async () =>
                {
                    var ga = new Algorytm(D, populationSize, generations, crossoverRate, mutationRate, tournamentSize, maxGenerations);
                    await ga.RunGeneticAlgorithmAsync(progress, cancellationToken);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd: {ex.Message}");
            }
            finally
            {
                toolStripProgressBar1.Value = 100; 
            }
        }
    }
}
