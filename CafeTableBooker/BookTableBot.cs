using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace CafeTableBooker
{
    public class BookTableBot : IBot
    {
        private DialogSet _dialogs;

        public BookTableBot()
        {
            _dialogs = new DialogSet();

            _dialogs.Add("booking", new BookTableControl());
            _dialogs.Add("firstRun",
                new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                         await dc.Context.SendActivity("Welcome! We need to ask a few questions to get started.");
                         await dc.Begin("booking",
                             new Dictionary<string, object> { { "message", "hello world" } });
                    },
                    async (dc, args, next) =>
                    {
                        // Note the use of ToNaturalLanguage() which will give friendly strings like "tomorrow" "yesterday" etc.
                        // It takes a datetime arg because the friendly strings are relative to a time - generally the current time

                        var timexProperty = new TimexProperty(args["bookingDateTime"].ToString());
                        var bookingDateTime = $"{timexProperty.ToNaturalLanguage(DateTime.Now)}";
                        var bookingLocation = args["bookingLocation"].ToString();
                        var bookingGuestCount = args["bookingGuestCount"].ToString();

                        await dc.Context.SendActivity($"Thanks, I have {bookingGuestCount} guests booked for our {bookingLocation} location for {bookingDateTime}.");
                        await dc.End();
                    }
                }
            );
        }

        public async Task OnTurn(ITurnContext turnContext)
        {
            try
            {
                switch (turnContext.Activity.Type)
                {
                    case ActivityTypes.Message:
                        var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                        var dc = _dialogs.CreateContext(turnContext, state);

                        await dc.Continue();

                        if (!turnContext.Responded)
                        {
                            await dc.Begin("firstRun");
                        }

                        break;

                    case ActivityTypes.ConversationUpdate:
                        foreach (var newMember in turnContext.Activity.MembersAdded)
                        {
                            if (newMember.Id != turnContext.Activity.Recipient.Id)
                            {
                                await turnContext.SendActivity("Hello and welcome to the book a table bot.");
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                await turnContext.SendActivity($"Exception: {e.Message}");
            }
        }
    }
}
