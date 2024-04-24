using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizProject.Models
{
    public class Questions
    {
        public string Text { get; set; } = string.Empty;
        public List<string> Answers { get; set; } = new();
        public int Answer { get; set; }
    }
}