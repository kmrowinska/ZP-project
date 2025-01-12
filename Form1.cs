using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace PROJEKT_ZAPIERDALANIE
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            P = new List<int>();
            D = new List<int>();
            random = new Random();
        }

        List<int> P, D;
        
        //zapis do pliku
        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog sFD = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt",
                Title = "Zapisz instancje do pliku .txt"
            };


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

                    // Parsowanie multizbioru z pliku
                    D = fileContent.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(int.Parse)
                                    .ToList();

                    // Blokowanie opcji generowania nowego multizbioru
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

            // Wyświetlanie oryginalnych danych w richTextBox1 i richTextBox2
            richTextBox1.Text = string.Join(" ", P);
            richTextBox2.Text = string.Join(" ", D);

            // Sprawdzenie warunków modyfikacji i ewentualne wprowadzenie błędów
            if (insertion > 0 || deletion > 0)
            {
                List<int> modifiedD = new List<int>(D); // Tworzymy kopię D, aby nie zmieniać oryginału
                Errors_in_D(modifiedD, insertion, deletion, len, count);
                richTextBox3.Text = string.Join(" ", modifiedD); // Wyświetlenie zmodyfikowanego multizbioru
            }
            else
            {
                richTextBox3.Text = string.Join(" ", D); // Jeśli brak modyfikacji, wyświetl oryginalne D
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
            // D.Clear();
            // richTextBox2.Clear();

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
            if (D != null && D.Any()) // Sprawdzanie, czy D jest wczytane i zawiera elementy
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
            numericUpDown3_ValueChanged(sender, e); // Wspólna logika dla obu kontrolek
        }




        Random random;

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
    }
}
