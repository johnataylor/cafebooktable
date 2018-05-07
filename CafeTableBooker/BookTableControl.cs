using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace CafeTableBooker
{
    public class BookTableControl : DialogContainer
    {
        public BookTableControl()
            : base("order")
        {
            var promptOptions = new ChoicePromptOptions
            {
                Choices = new List<Choice>
                {
                    new Choice { Value = "Seattle" },
                    new Choice { Value = "Bellevue" },
                    new Choice { Value = "Renton" },
                }
            };

            Dialogs.Add("order",
                new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                        // The dictionary passed into Begin is available here as the args
                        // if you want anything saved from that then add it to the State

                        dc.ActiveDialog.State = new Dictionary<string, object>();
                        await dc.Prompt("timexPrompt", "When would you like to arrive? (We open at 4PM.)",
                            new PromptOptions { RetryPromptString = "Please pick a date in the future and a time in the evening." });
                    },
                    async (dc, args, next) =>
                    {
                        var timexResult = (TimexResult)args;
                        dc.ActiveDialog.State["bookingDateTime"] = timexResult.Resolutions.First();
                        await dc.Prompt("choicePrompt", "Which of our locations would you like?", promptOptions);
                    },
                    async (dc, args, next) =>
                    {
                        var choiceResult = (FoundChoice)args["Value"];
                        dc.ActiveDialog.State["bookingLocation"] = choiceResult.Value;

                        await dc.Prompt("numberPrompt", "How many in your party?");
                    },
                    async (dc, args, next) =>
                    {
                        dc.ActiveDialog.State["bookingGuestCount"] = args["Value"];
                        await dc.End(dc.ActiveDialog.State);
                    }
                }
            );
            // TimexPrompt is a very close copy of the existing DateTimePrompt - but manages to correctly handle multiple resolutions
            Dialogs.Add("timexPrompt", new TimexPrompt(Culture.English, TimexValidator));
            Dialogs.Add("choicePrompt", new Microsoft.Bot.Builder.Dialogs.ChoicePrompt(Culture.English) { Style = ListStyle.Inline });
            Dialogs.Add("numberPrompt", new Microsoft.Bot.Builder.Dialogs.NumberPrompt<int>(Culture.English));
        }

        // The notion of a Validator is a standard pattern across all the Prompts
        private static Task TimexValidator(ITurnContext context, TimexResult value)
        {
            var cadidates = value.Resolutions;

            var constraints = new[] { TimexCreator.ThisWeek(), TimexCreator.NextWeek(), TimexCreator.Evening };

            var resolutions = TimexRangeResolver.Evaluate(cadidates, constraints);

            if (resolutions.Count == 0)
            {
                value.Resolutions = new string[] {};
                value.Status = PromptStatus.OutOfRange;
            }
            else
            {
                value.Resolutions = new[] { resolutions.First().TimexValue };
                value.Status = PromptStatus.Recognized;
            }

            return Task.CompletedTask;
        }
    }
}
