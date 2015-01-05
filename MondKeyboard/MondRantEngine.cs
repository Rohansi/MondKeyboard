using Mond.Binding;
using Rant;
using Rant.Vocabulary;

namespace MondKeyboard
{
    [MondClass("RantEngine")]
    public class MondRantEngine
    {
        private readonly RantEngine _rant;

        [MondConstructor]
        public MondRantEngine(bool nsfw = true)
        {
            _rant = new RantEngine("dictionary", nsfw ? NsfwFilter.Allow : NsfwFilter.Disallow);
        }

        [MondFunction("run")]
        public string Run(string input)
        {
            return _rant.Do(input, 2000);
        }
    }
}
