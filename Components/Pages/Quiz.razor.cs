// quiz.razor.cs
using Microsoft.AspNetCore.Components;
using QuizProject.Models;
using System.Threading.Tasks;

namespace QuizProject.Components.Pages
{
    public partial class QuizModel : ComponentBase
    {
        [Parameter]
        public string? UserName { get; set; }
        [Parameter]
        public List<Questions>? UserQuestions { get; set; }
        [Parameter]
        public int UserNumberOfQuestions { get; set; }
        [Parameter]
        public int UserTimeLimit { get; set; }
        [Parameter]
        public string? uniqueIdString { get; set; }
        [Parameter]
        public bool TakeQuiz { get; set; }
        [Parameter]
        public EventCallback<bool> TakeQuizChanged { get; set; }
        public int SelectedAnswer { get; set; } // This property will hold the value of the selected answer
        public int CurrentAnswer { get; set; }
        public List<Questions>? CurrentQuestionsAsked { get; set; }
        public int CurrentQuestionIndex { get; set; } // Track the index of the current question
        public bool IsTimerRunning { get; set; } = true; // Indicates if the timer is currently running
        public TimeSpan RemainingTime { get; set; } // Remaining time on the timer
        public int CorrectAnswers { get; set; }
        public string? FinishedTime { get; set; }
        public Timer? timer { get; set; }
        public bool quizCompleted {get;set;} = false;
        public string logFilePath = "quizlog.dat"; // Path to the log file
        public List<string>? PreviousQuizAttempts { get; set;}
        public bool Cleared { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Cleared = false;
            SelectedAnswer = 0;
            CurrentQuestionIndex = 0;

            if (UserQuestions != null)
            {
                CurrentQuestionsAsked = await RandomizeAndReturnCorrectNumOfQuestions();
            }

            StartTimer();
        }

        public async Task<List<Questions>> RandomizeAndReturnCorrectNumOfQuestions()
        {
            // Shuffle the list of all questions
            List<Questions> shuffledQuestions = Shuffle(UserQuestions);

            // Select the first 'numberOfQuestions' questions from the shuffled list
            List<Questions> selectedQuestions = shuffledQuestions.Take(UserNumberOfQuestions).ToList();

            return selectedQuestions;
        }

        // Method to shuffle a list
        public List<T> Shuffle<T>(List<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }

        public void SubmitAnswer()
        {
            if (CurrentAnswer == SelectedAnswer)
            {
                CorrectAnswers++;
            }
            else{

            }
            
            // Move to the next question
            CurrentQuestionIndex++;
            
            // Reset the selected answer for the next question
            SelectedAnswer = -1;
        }

        public void StartTimer()
        {
            // Set the remaining time to UserTimeLimit
            RemainingTime = TimeSpan.FromSeconds(UserTimeLimit);
            
            // Create and start the timer
            int intervalMilliseconds = (int)TimeSpan.FromSeconds(1).TotalMilliseconds;
            timer = new Timer(TimerCallback, null, 0, intervalMilliseconds);
        }

       public async void TimerCallback(object state)
        {
            RemainingTime = RemainingTime.Subtract(TimeSpan.FromSeconds(1));
            if (RemainingTime.TotalSeconds <= 0)
            {
                await InvokeAsync(() => EndQuiz());
            }
            else
            {
                TimeSpan elapsedTime = TimeSpan.FromSeconds(UserTimeLimit) - RemainingTime;
                FinishedTime = elapsedTime.ToString(@"mm\:ss"); // Store the elapsed time
                await InvokeAsync(StateHasChanged); // Notify the UI to update
                if(quizCompleted) 
                {
                    IsTimerRunning = true;
                    timer.Dispose();
                    await LogQuizAttempt();
                }
            }
        }

        public async Task EndQuiz()
        {
            IsTimerRunning = false; // Stop the timer
            await InvokeAsync(StateHasChanged); // Notify the UI to update
            
            // Stop the timer
            timer.Dispose();

            await LogQuizAttempt();
        }

        public async Task LogQuizAttempt()
        {
            var correctPercentage = ((float)CorrectAnswers / (float)CurrentQuestionsAsked.Count) * 100;

string logMessage 
= @$"
User ID: {uniqueIdString}
User Name: {UserName}
{DateTime.Now}
Total Questions: {CurrentQuestionsAsked.Count}
Questions Answered: {CurrentQuestionIndex}
Correct: {CorrectAnswers}
Percentage Correct:{correctPercentage}%
Time Limit: {UserTimeLimit} seconds
Finished Exam Time: {FinishedTime}
*------------------------------*
";

            try
            {
                using (StreamWriter writer = File.AppendText(logFilePath))
                {
                    writer.WriteLine(logMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging quiz attempt: {ex.Message}");
            }
        }

        public void DisplayPreviousQuizAttempts()
        {
            Cleared = false;
            bool attemptsFound = false;
            PreviousQuizAttempts = new();
            
            try
            {
                using (StreamReader reader = new StreamReader(logFilePath))
                {
                    string? line;
                    bool foundMatchingId = false;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Contains($"User ID: {uniqueIdString}"))
                        {
                            foundMatchingId = true;
                            attemptsFound = true;
                        }
                        else if (foundMatchingId)
                        {
                            PreviousQuizAttempts.Add(line);
                            if (line.Contains("*------------------------------*"))
                            {
                                // Break the loop if the end delimiter is found
                                foundMatchingId = false;
                            }
                        }
                    }
                }

                if (!attemptsFound)
                {
                    Console.WriteLine("\nNo previous quiz attempts found.");
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Log file not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error displaying previous quiz attempts: {ex.Message}");
            }
        }

        public void ClearResult()
        {
            Cleared = true;
        }

        public async Task TakeNewQuiz()
        {
            Console.WriteLine(TakeQuiz);
            TakeQuiz = !TakeQuiz;
            Console.WriteLine(TakeQuiz);
            await TakeQuizChanged.InvokeAsync(TakeQuiz);
            StateHasChanged();
        }
    }
}