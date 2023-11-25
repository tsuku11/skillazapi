using System;
using System.Net.Http.Json;
using System.Text.Json;

namespace HttpIntegrationTemplate;





public class CandidateInfo
{
    public string Skills { get; set; }
    public string Citizenship { get; set; }
    public List<WorkExperience> WorkExperience { get; set; }
    public List<DrivingExperiences> DrivingExperiences { get; set; }
    public string VacancyId { get; internal set; }
}

public class VacancyInfo
{
    public string Name { get; set; }
    public string PropertyName { get; set; }
    public int WorkExperience { get; set; }
    public string RequiredCitizenship { get; set; }
    public bool NeedDriverLicense { get; set; }

}
public class WorkExperience
{
    public string CompanyName { get; set; }
    public string Position { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalMonths { get; set; }
    public string Description { get; set; }
    public string Industries { get; set; }
    public string City { get; set; }
    public string EmploymentType { get; set; }
}

public class DrivingExperiences
{
    public bool HasPersonalCar { get; set; }

    public string DrivingLicense { get; set; }
    
}

public class CandidateService
{
    private readonly HttpClient _httpClient;

    public CandidateService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public void CheckCandidate(string id)
    {
        var candidate = GetCandidateInfo(id);
        var vacancy = GetVacancyInfo(candidate.VacancyId);

        if (CalculateMatching(candidate, vacancy))
        {
            AddCommentToCandidate(id, "Подходит");
        }
        else
        {
            AddCommentToCandidate(id, "Не подходит");
        }
    }

    private bool CalculateMatching(CandidateInfo candidateInfo, VacancyInfo vacancyInfo)
    {
        double skillIntersectionCount = candidateInfo.Skills.Intersect(vacancyInfo.PropertyName).Count();

        double skillIntersectionPercentage = (skillIntersectionCount / vacancyInfo.PropertyName.Length) * 100;

        if (skillIntersectionPercentage < 70)
        {
            return false;
        }

        // Проверка опыта работы
        int experienceDifference = vacancyInfo.WorkExperience - candidateInfo.WorkExperience.Sum(x => x.TotalMonths);
        if (experienceDifference > 6)
        {
            return false;
        }

        // Проверка гражданства
        if (vacancyInfo.RequiredCitizenship != "Any" && vacancyInfo.RequiredCitizenship != candidateInfo.Citizenship)
        {
            return false;
        }

        
        return true;
    }

    public CandidateInfo GetCandidateInfo(string id)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, "/open-api/objects/candidates/filtered")
        {
            Content = JsonContent.Create(new CandidateInfoRequest
            {
                Ids = new[] { id }
            }, options: new() { PropertyNamingPolicy = null })
        };

        var response = _httpClient.Send(message);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Ошибка запроса");
        }

        var deserializedResponse = JsonSerializer.Deserialize<CandidateInfoResponse>(response.Content.ReadAsStream());

        return deserializedResponse.Items.FirstOrDefault() ?? throw new Exception("Не найден кандидат");
    }

    private VacancyInfo GetVacancyInfo(string vacancyId)
    {
        //todo: используйте _httpClient для получения вакансии
        return new VacancyInfo();
    }

    private void AddCommentToCandidate(string id, string text)
    {
        //todo: используйте _httpClient для отправки комментария 
    }
}

public class CandidateInfoRequest
{
    public string[] Ids { get; set; }
    public CandidateCommonCvInfo CommonCVInfo { get; set; }
}

public class CandidateInfoResponse
{
    public CandidateInfo[] Items { get; set; }
}



public class CandidateCommonCvInfo
{
    public string[] Citizenship { get; set; }
    public CandidateWorkExperience[] WorkExperience { get; set; }
    //todo: добавьте остальные поля на основе swagger
}

public class CandidateWorkExperience
{
    public string CompanyName { get; set; }
    private int TotalMonths { get; set; }
}

public class CandidateNote
{
    public string Id { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
}
