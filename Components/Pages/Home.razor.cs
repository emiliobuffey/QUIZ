using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using QuizProject.Models;

namespace QuizProject.Components.Pages
{
    public partial class HomeModel : ComponentBase
    {
        public string? Name { get; set; }
        public bool ErrorSaving { get; set; }
        public bool ValidInfoSaving { get; set; }
        public IList<IBrowserFile>? files { get; set; }
        public int NumberOfQuestionsToAsk {get; set; } = 0;
        public int TimeLimit { get; set; } = 0;
        public bool QuestionsLoaded { get; set; } = false;
        public List<Questions>? Questions { get; set; }
        public static string? uniqueIdString { get; set; }
        public bool ProblemReadingFile { get; set; }
        public bool TakeTheQuiz { get; set; }

        protected override void OnInitialized()
        { 
            TakeTheQuiz = false;
            ErrorSaving = false;
            ValidInfoSaving = false;
            ProblemReadingFile = false;
            files = new List<IBrowserFile>();
            Questions = new();
        }

        public void UploadFiles(IBrowserFile file)
        {
            ProblemReadingFile = false;
            // only want to have 1 file
            files = new List<IBrowserFile>();
            files.Add(file);
            Console.WriteLine("count of files = " + files.Count);
        }

        public async void SaveClicked()
        {   
            ProblemReadingFile = false;

            if(!string.IsNullOrEmpty(Name) && files.Any() && NumberOfQuestionsToAsk > 0 && TimeLimit > 0 && !ProblemReadingFile)
            {
                ErrorSaving = false;
                ValidInfoSaving = true;
                // Proceed with saving
                Questions =  await ReadQuestions(files);

                // Check if questions were successfully loaded
                if (Questions.Count > 0)
                {
                    // All good
                    await Task.Delay(1500);
                    ValidInfoSaving = false;

                    // Generate a new Guid for the user
                    if(string.IsNullOrEmpty(uniqueIdString))
                    {
                        Guid uniqueId = Guid.NewGuid();
                        uniqueIdString = uniqueId.ToString();
                    }

                    // Load quiz
                    QuestionsLoaded = true;
                    TakeTheQuiz = false;
                    StateHasChanged();
                }
                else
                {
                    Console.WriteLine("Quiz cannot be started without questions.");
                    Console.WriteLine("Please make sure the quiz file has the appropriate formatting and tags (@Q,@A,@E).\n");
                    ErrorSaving = true;
                    ProblemReadingFile = true;
                    ValidInfoSaving = false;
                    StateHasChanged();
                    return;
                }
            }
            else
            {
                Console.WriteLine("Error: Name, Questions, TimeLimit, and File are required.");
                ErrorSaving = true;
                ProblemReadingFile = true;
                ValidInfoSaving = false;
                StateHasChanged();
            }

            Console.WriteLine(ValidInfoSaving);
        }

        public async Task<List<Questions>> ReadQuestions(IList<IBrowserFile>? files)
        {
            List<Questions> questions = new List<Questions>();

            if (files == null || files.Count == 0)
            {
                Console.WriteLine("No files provided.");
                return questions;
            }

            IBrowserFile file = files[0]; // Assuming only one file is provided

            try
            {
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    string line;
                    string questionText = string.Empty;
                    int answer = 0;

                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (line.StartsWith("*") || string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        if (line.StartsWith("@Q"))
                        {
                            questionText = string.Empty;
                            List<string> answers = new List<string>(); // Create a new list for each question

                            while ((line = await reader.ReadLineAsync()) != null && !line.StartsWith("@A"))
                            {
                                if (!line.StartsWith("@Q"))
                                {
                                    // store the question 
                                    questionText += line + " ";
                                }
                            }

                            if (line != null && line.StartsWith("@A"))
                            {
                                if ((line = await reader.ReadLineAsync()) != null && int.TryParse(line, out int ans))
                                {
                                    // store the correct answer
                                    answer = ans;
                                }
                                else
                                {
                                    Console.WriteLine("The answer in the file was not an integer! I set the correct answer to zero to handle the invalid input.");
                                    answer = 0;
                                }

                                while ((line = await reader.ReadLineAsync()) != null && !line.StartsWith("@E"))
                                {
                                    // Store the answers
                                    answers.Add(line);
                                }
                            }

                            // Store the question, possible answers, and the correct answer
                            questions.Add(new Questions { Text = questionText, Answers = answers, Answer = answer });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading questions: {ex.Message}");
            }
            return questions;
        }
        public async Task HandleTakeQuizChanged(bool newValue)
        {
            TakeTheQuiz = newValue;
        }

        public void NewUserClicked()
        {   
            TakeTheQuiz = false;
            Name = "";
            NumberOfQuestionsToAsk = 0;
            TimeLimit = 0;
            files = new List<IBrowserFile>();
            Guid uniqueId = Guid.NewGuid();
            uniqueIdString = uniqueId.ToString();
            Questions = new();
            QuestionsLoaded = false;
            StateHasChanged();
        }
    }
}