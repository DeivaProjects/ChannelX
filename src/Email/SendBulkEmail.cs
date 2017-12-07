using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Web;

using Quartz;
using ChannelX.Email;
using ChannelX.Data;
using ChannelX.Models;
using ChannelX.Hubs;
using ChannelX.Models.Chat;
using ChannelX.Models.Channel;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Hosting;

using Newtonsoft.Json;
using ChannelX.Models.Trackers;
using ChannelX.Redis;
using StackExchange.Redis;

public class SendBulkEmail : IJob
{
    readonly DatabaseContext _db;
    readonly IEmailSender _emailSender;
    readonly UserTracker _tracker;
    readonly StackExchange.Redis.IDatabase _redis_db;
    readonly IRedisConnectionFactory _fact;
    readonly IHostingEnvironment _env;

    public SendBulkEmail(IEmailSender emailSender, DatabaseContext db, UserTracker tracker,  IRedisConnectionFactory fact, IHostingEnvironment env)
    {
        _db = db;
        _emailSender = emailSender;
        _tracker = tracker;
        _fact = fact;
        _env = env;

        if(fact.IsConnected)
        {
            Console.WriteLine("Redis connected!");
            _redis_db = fact.Connection().GetDatabase();
        }
    }
    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("Trying to execute the job.");
        // GenerateBulkEmailAsync();
        // SMTP TEST START
        // await _emailSender.SendEmailAsync("asdfgh1453@mynet.com", "subject", "Enter email body here");
        // -----SMTP TEST END
    }

    public async Task GenerateBulkEmailAsync()
    {
        // var user = _tracker.Remove(Context.Connection);

        // Fetch all channels
        var channel_list = _db.Channels.Include(i => i.Users).ToList();
        foreach (var channel in channel_list)
        {
            Console.WriteLine(channel.Title);
            // Console.WriteLine(channel.Users);

            // For all users in the currently fetched channel
            foreach (var user in channel.Users)
            {
                Console.WriteLine(user.UserId);
                var sent_message_count = 0;
                var sent_email_body = "";
                var last_seen_time = DateTime.Now;
                List<TextModel> message_list = new List<TextModel>();
                // If the redis DB is connected succesfully
                if(_fact.IsConnected)
                {
                    Console.WriteLine("Redis connected!!!!!");
                    // last seen is updated ondisconnectedasync
                    var test_data = _redis_db.HashGet("LastSeen" + user.ChannelId, user.UserId);
                    Console.WriteLine("Plz: " + test_data);
                    last_seen_time = Convert.ToDateTime(test_data);

                    // Get all the messages in that channel
                    // Compare them with last seen, and append them to list
                    var messages = _redis_db.ListRange(user.ChannelId.ToString(),0,-1);
                    foreach(var message in messages)
                    {
                        TextModel text = JsonConvert.DeserializeObject<TextModel>(message);

                        System.Diagnostics.Debug.WriteLine("Message:");
                        Console.WriteLine(text.Content);
                        Console.WriteLine(text.User.Name);
                        Console.WriteLine(text.SentTime);
                        var sent_time = Convert.ToDateTime(text.SentTime);
                        if( sent_time < last_seen_time )
                        {
                            // Add message to bulk email content
                            sent_email_body+=text.Content;
                            // Add message to list
                            message_list.Add(text);
                            // Increment the counter
                            sent_message_count++;
                        }
                    }
                    // If no message is fetched no need to send a mail
                    if (sent_message_count == 0)
                    {
                        Console.WriteLine("No need to send a mail.");
                    }
                    else
                    {
                        Console.WriteLine("Email body: " + sent_email_body);
                        var finalized_mail_body = GetFormattedMessage(message_list, user);
                        Console.WriteLine("--");
                        Console.WriteLine(finalized_mail_body);
                        Console.WriteLine("--");
                        // Send the email to user
                        await _emailSender.SendEmailAsync("itu.channelx@gmail.com", "subject", finalized_mail_body);
                        break;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("An error occured during connecting to Redis DB.");
                }
            }
        }
        Console.WriteLine("Channels succesfully gotten!");
        Console.WriteLine("----------------------------");
    }
    public string GetFormattedMessage(List<TextModel> message_list, ChannelUser current_user)
    {
        var finalized_message = "";
        Console.WriteLine(_env.ContentRootPath + "\\Email\\heml_upper.txt");
        // Add upper part of the mail to bulk
        finalized_message+= System.IO.File.ReadAllText(_env.ContentRootPath + "\\Email\\heml_upper.txt");
        foreach(var mes in message_list)
        {
            if (mes.User.UserId == current_user.UserId)
            {
                Console.WriteLine("Heleluya!");
                Console.WriteLine(mes.User.Name);
                Console.WriteLine(mes.Content);
                finalized_message += System.IO.File.ReadAllText(_env.ContentRootPath + "\\Email\\div_row_upper.txt");
                finalized_message += "<div large=\"12\">" + mes.User.Name + "<br>" + mes.Content + "</div>";
                finalized_message += System.IO.File.ReadAllText(_env.ContentRootPath + "\\Email\\div_row_lower.txt");
            }
            else
            {
                Console.WriteLine(mes.User.Name);
                Console.WriteLine(mes.Content);
                finalized_message += System.IO.File.ReadAllText(_env.ContentRootPath + "\\Email\\div_row_upper.txt");
                finalized_message += "<div large=\"12\">" + mes.User.Name + "<br>" + mes.Content + "</div>";
                finalized_message += System.IO.File.ReadAllText(_env.ContentRootPath + "\\Email\\div_row_lower.txt");
            }
        }
        // Add lower part of the mail to bulk
        finalized_message+= System.IO.File.ReadAllText(_env.ContentRootPath + "\\Email\\heml_lower.txt");
        return finalized_message;
    }
}