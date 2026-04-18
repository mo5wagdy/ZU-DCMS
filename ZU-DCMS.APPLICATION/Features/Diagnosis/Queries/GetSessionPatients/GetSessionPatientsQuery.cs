namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Queries.GetSessionPatients
{
    public class GetSessionPatientsQuery
    {
        public int SessionId { get; set; }
        public string InternDoctorId { get; set; } = null!;
    }
}
