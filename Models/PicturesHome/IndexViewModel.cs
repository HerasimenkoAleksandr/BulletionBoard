namespace BulletionBoard.Models.PicturesHome
{
    public class IndexViewModel
    {
       public List<PicturesFindModel> PicturesModels { get; set; }
        public IndexViewModel()
        {
            String picSport = Path.Combine("pictures/sport.jpg");
            String picJob = Path.Combine("pictures/job.jpg");
            String picCloth = Path.Combine("pictures/cloth.jpg");
            String picOther = Path.Combine("pictures/other.jpg");
            String picreaRealestate = Path.Combine("pictures/realestate.jpg");
            String picServices = Path.Combine("pictures/Services.jpg");

            PicturesModels =new List<PicturesFindModel> {
                new PicturesFindModel { PathToPictures = picSport, Name = "Спорт", NameForTitle="Sport", Id="sportId" },
                new PicturesFindModel { PathToPictures = picJob, Name = "Робота",  NameForTitle="Job", Id="jobId"},
                new PicturesFindModel { PathToPictures = picCloth, Name = "Одяг" , NameForTitle="Cloth", Id="clothId"},
                new PicturesFindModel { PathToPictures = picOther, Name = "Інше" , NameForTitle="Other", Id="otherId"},
                new PicturesFindModel { PathToPictures = picreaRealestate, Name = "Нерухомість" , NameForTitle="Realestate", Id="realestateId"},
                new PicturesFindModel { PathToPictures = picServices, Name = "Послуги" , NameForTitle="Services", Id="servicesId"}
                            

             };
        }
    }
    
}
