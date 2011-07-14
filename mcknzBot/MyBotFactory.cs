using Compete.Bot;

/********************************************
 * 
 * This file should not be modified.
 * Code your move logic in MyBot.cs
 * 
 ********************************************/
namespace RockPaperAzure
{
    public class MyBotFactory : IBotFactory
    {
        public IBot CreateBot() { return new mcknz(); }
    }
}