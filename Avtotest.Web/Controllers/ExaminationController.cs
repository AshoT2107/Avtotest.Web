using Avtotest.Web.Models;
using Avtotest.Web.Repositories;
using Avtotest.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Avtotest.Web.Controllers
{
    public class ExaminationController : Controller
    {
        private QuestionsRepository _questionsRepository;
        private TicketsRepository _ticketsRepository;
        private UserService _userService;
        private const int TicketQuestionsCount = 20;
        public ExaminationController(QuestionsRepository questionsRepository,TicketsRepository ticketsRepository,UserService userService)
        {
            _questionsRepository = questionsRepository;
            _ticketsRepository = ticketsRepository;
            _userService = userService;
        }
        public string QuestionsCount()
        {
           return _questionsRepository.ConvertQuestionToJson().Count().ToString();
        }
        public IActionResult Index()
        {
            var user = _userService.GetUserFromCookie(HttpContext);
            if (user == null)
                return RedirectToAction("SignIn", "Users");
            var ticket = CreateRandomTicket(user);
            return View(ticket);
        }
        private Ticket CreateRandomTicket(User user)
        {
            var TicketsCount = _questionsRepository.GetQuestionsCount() / TicketQuestionsCount;
            var randomNuber = new Random();
            var fromIndex = randomNuber.Next(1, TicketsCount) * TicketQuestionsCount;
            var ticket = new Ticket(user.Index, fromIndex, TicketQuestionsCount);
            _ticketsRepository.InsertTicket(ticket);
            ticket.Id = _ticketsRepository.GetLastRowId();
            return ticket;
        }

        [Route("tickets/{ticketId}")]
        [Route("tickets/{ticketId}/questions/{questionId}")]
        [Route("tickets/{ticketId}/questions/{questionId}/choices/{choiceId}")]
        public IActionResult Exam(int ticketId,int? questionId = null,int? choiceId=null)
        {
            var user = _userService.GetUserFromCookie(HttpContext);
            if (user == null)
            {
                return RedirectToAction("SignIn", "User");
            }
            var ticket = _ticketsRepository.GetTicketById(ticketId, user.Index);
            questionId = questionId ?? ticket.FromIndex;
            if (ticket.FromIndex<=questionId && ticket.QuestionsCount + ticket.FromIndex > questionId)
            {
                
                var question = _questionsRepository.GetQuestionById(questionId.Value);
                var t = _ticketsRepository.GetTicketDataByTicketId(ticket.Id);
                ViewBag.TicketData = t;
                var _ticketData = _ticketsRepository.GetTicketDataByQuestionId(ticket.Id,question.Id);
                var _choiceId = (int?)null;
                var _answer = false;
                if (_ticketData != null)
                {
                    _choiceId = _ticketData.ChoiceId;
                    _answer = _ticketData.Answer;
                }
                else if (choiceId != null)
                {
                    var answer = question.Choices!.FirstOrDefault(ch => ch.Id == choiceId)!.Answer;
                    var ticketData = new TicketData
                    {
                        TicketId = ticketId,
                        QuestionId = question.Id,
                        ChoiceId = choiceId.Value,
                        Answer = answer
                    };
                    _ticketsRepository.InsertTicketData(ticketData);
                    _choiceId = choiceId;
                    _answer = answer;
                }
                if (_answer)
                {
                    _ticketsRepository.UpdateTicketCorrectAnswerCount(ticketId);
                }
                if(ticket.QuestionsCount == _ticketsRepository.GetTicketAnswerCount(ticket.Id))
                {
                    return RedirectToAction("ExamResult", new {ticketId = ticket.Id});
                }

                ViewBag.Ticket = ticket;
                ViewBag.ChoiceId = _choiceId;
                ViewBag.Answer = _answer;
                return View(question);
            }
            return NotFound();
        }
        public IActionResult ExamResult(int ticketId)
        {
            var user = _userService.GetUserFromCookie(HttpContext);
            if(user == null)
            {
                return RedirectToAction("SignIn", "User");
            }
            var ticket = _ticketsRepository.GetTicketById(ticketId, user.Index);
            return View(ticket);
        }

    }
}
