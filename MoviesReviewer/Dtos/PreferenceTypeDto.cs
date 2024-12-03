namespace MoviesReviewer.Dtos
{
    public class PreferenceTypeDto
    {

        public PreferenceTypeDto(string type, string display) { 
            Type = type;
            Display = display;
        }

        public string Type { get; set; }

        public string Display { get; set; }
    }
}
