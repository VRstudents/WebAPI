﻿namespace WebAPI.Models.App
{
    public class User
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public int FinishedSignUP { get; set; }
        public string Role { get; set; }
        public string Code { get; set; }
    }
}