﻿namespace FinderScraper.Objects
{
    public class Memorial
    {
        public int MemorialId { get; set; } = 0;
        public bool IsVeteran { get; set; } = false;
        public bool IsFamous { get; set; } = false;

        public string? FirstName { get; set; } = null;
        public string? MiddleName { get; set; } = null;
        public string? LastName { get; set; } = null;
        public string? FullName { get; set; } = null;
        public string? BirthCityName { get; set; } = null;
        public string? DeathCityName { get; set; } = null;

        public string? BirthDate { get; set; } = null;
        public string? DeathDate { get; set; } = null;

        public string[]? Parents { get; set; } = null;
        public string?[]? Spouses { get; set; } = null;
        public string[]? Siblings { get; set; } = null;
        public string[]? Children { get; set; } = null;

        public float[]? Location { get; set; } = null;


        public Memorial(int memorialId = 0, string? firstName = null, string? middleName = null, string? lastName = null, string? birthCityName = null, string? deathCityName = null, string? fullName = null, string? birthDate = null, string? deathDate = null, string[]? parents = null, string[]? spouses = null, string[]? siblings = null, string[]? children = null, float[]? location = null, bool isVeteran = false, bool isFamouse = false)
        {
            MemorialId = memorialId;
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
            BirthCityName = birthCityName;
            DeathCityName = deathCityName;
            FullName = fullName;
            BirthDate = birthDate;
            DeathDate = deathDate;
            Parents = parents;
            Spouses = spouses;
            Siblings = siblings;
            Children = children;
            Location = location;
            IsVeteran = isVeteran;
            IsFamous = isFamouse;
        }
    }

}
