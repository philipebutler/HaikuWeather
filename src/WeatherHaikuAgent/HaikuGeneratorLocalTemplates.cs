using WeatherHaikuAgent.Models;

namespace WeatherHaikuAgent;

public class HaikuGeneratorLocalTemplates
{
    private readonly string _rotationMode;
    private readonly Random _random = new();

    private static readonly Dictionary<string, List<string>> _haikuTemplates = new()
    {
        ["Frost Monk"] = new List<string>
        {
            "Breath freezes mid-air\nSilence deeper than the cold\nStillness teaches peace",
            "Ice crystals whisper\nAncient truths in bitter wind\nThe void calls me home",
            "Fingers numb, mind clear\nIn the frost I find myself\nCold becomes the way",
            "Bones ache with wisdom\nEach degree below zero\nBrings me to the truth",
            "Winter's harsh embrace\nStrips away all that is false\nOnly essence stays",
            "Frost paints sacred texts\nOn windows of perception\nRead them while you can"
        },

        ["Snow Comedian"] = new List<string>
        {
            "My car won't start, great\nGuess I'm working from my bed\nSnow day, baby, yeah!",
            "Penguin weather, folks\nWaddling to the mailbox\nSlip, fall, laugh, repeat",
            "Snowman's got it made\nNo job, no bills, just chillin'\nLiving his best life",
            "Winter: nature's way\nOf saying 'Stay inside, champ\nNetflix needs you now'",
            "Ice on the driveway\nPracticing my figure eights\nOr just trying to walk",
            "Frozen nose hair, check\nCan't feel my toes anymore\nAh, the joys of cold"
        },

        ["Mud Philosopher"] = new List<string>
        {
            "Between ice and warmth\nWe dwell in the gray question\nNeither here nor there",
            "Mud beneath my boots\nNeither solid nor liquid\nLife is transition",
            "Spring pretends to come\nWinter holds on stubbornly\nWe exist between",
            "Forty degrees asks:\nWhy do we measure comfort?\nWhat is warm enough?",
            "The in-between time\nWhen jackets feel uncertain\nSo too do our souls",
            "Neither frost nor sun\nThe weather of contemplation\nWisdom loves the gray"
        },

        ["Porch Poet"] = new List<string>
        {
            "Gentle breeze at dusk\nLemonade and rocking chair\nThis is the sweet spot",
            "Windows open wide\nCurtains dancing with the wind\nPerfection exists",
            "Neither coat nor fan\nJust the air upon my skin\nNature's gift to us",
            "Barefoot on the deck\nEvening sun on my shoulders\nGratitude flows free",
            "Coffee on the porch\nMorning birds sing their old songs\nAll is right today",
            "Golden hour glows\nTemperature matches my soul\nI could stay right here"
        },

        ["Sun Hypeman"] = new List<string>
        {
            "LET'S GOOOOO! It's hot!\nPool party energy, yes!\nVitamin D blast!",
            "Sun's out, guns out, bro\nShades on, vibes immaculate\nSummer mode: ENGAGED",
            "Eighty-five degrees\nOf pure unadulterated\nBBQ weather, FAM",
            "Ice cream melts so fast\nBut my spirit soars higher\nHOT WEATHER HYPE TRAIN",
            "Sweat is just liquid\nSuccess leaving your body\nEMBRACE THE HEAT, KING",
            "Tank top season, YES\nFlip flops and good vibes only\nTHE SUN IS OUR FRIEND"
        },

        ["Heat Dramatic"] = new List<string>
        {
            "The sun has betrayed\nUs all, scorching without mercy\nThis is the ending",
            "Hell's doorstep, they call\nThis temperature, and yet I\nMust exist within",
            "Asphalt melts, dreams melt\nHope itself evaporates\nInto cruel bright sky",
            "Air conditioner\nBreathes its last, and so shall I\nTragedy unfolds",
            "Why do I live here?\nA question asked by all who\nDare step outside now",
            "The heat is a stage\nWe perform our suffering\nAudience: the sun",
            "Ninety-five degrees\nNature's way of asking: Why?\nWhy did you stay here?"
        }
    };

    public HaikuGeneratorLocalTemplates(string rotationMode = "Deterministic")
    {
        _rotationMode = rotationMode;
    }

    public string GenerateHaiku(WeatherContext context)
    {
        if (!_haikuTemplates.ContainsKey(context.Persona))
        {
            return "No persona found here\nSomething went wrong, oh dear me\nDefault haiku time";
        }

        var templates = _haikuTemplates[context.Persona];
        
        int index;
        if (_rotationMode == "Random")
        {
            index = _random.Next(templates.Count);
        }
        else
        {
            var seed = $"{context.Persona}_{context.Timestamp.Date}_{(int)context.TemperatureF}";
            var hash = seed.GetHashCode();
            index = Math.Abs(hash) % templates.Count;
        }

        return templates[index];
    }
}
