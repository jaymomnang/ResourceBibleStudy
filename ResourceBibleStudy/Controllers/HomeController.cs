﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using ResourceBibleStudy.Core.Dto;
using ResourceBibleStudy.Helpers;
using ResourceBibleStudy.Repository.Dao;

namespace ResourceBibleStudy.Controllers
{
    public class HomeController : BaseController
    {
        private readonly BibleRepository _bibleRepository;
        static Bible BibleContent { get; set; }

        public HomeController()
        {
            _bibleRepository = new BibleRepository();
        }
        public ActionResult Index(string translation = "TMSG")
        {
            ViewBag.Title = "Resource Bible Study";
            BibleContent = _bibleRepository.GetBible();
            ViewBag.BibleContent = BibleContent;

            return View();
        }

        [ActionName("search_book")]
        public string GetSelectedBook(int bookId = 1, int bookChapter = 0, int pageNumber = 1)
        {
            var readingResult = new StringBuilder();
            var firstTwelve = new List<Verse>();

            var paginInfo = QueryHelper.GetPagingRowNumber(pageNumber, 7);

            var selectedBook = _bibleRepository.GetBible().Books.FirstOrDefault(x => x.Id == bookId);
            ViewBag.Chapter = bookChapter;

            if (pageNumber == 1 && bookChapter == 0)
            {
                ViewBag.BookName = selectedBook.BookName;

                readingResult.Append(RenderPartialViewToString("_ChapterTableOfContent", selectedBook));
            }
            else
            {
                ViewBag.BookName = selectedBook.BookName;

                var chapter = selectedBook.BookChapter.Where(x => x.ChapterId == bookChapter).FirstOrDefault();
                firstTwelve.AddRange(chapter.ChapterVerses.Where(x => x.Id >= paginInfo.RowStart && x.Id < paginInfo.RowEnd));
                readingResult.Append(RenderPartialViewToString("_BookContent", firstTwelve));
            }

            return readingResult.ToString();
        }

        public ActionResult Book(int bookId = 1)
        {
            ViewBag.Book = _bibleRepository.GetBible().Books.FirstOrDefault(x => x.Id == bookId);

            return View();
        }
        [ActionName("back_to_book")]
        public ActionResult BackToBook(int chapterId = 1, int bookId = 1, string direction = "")
        {


            var book = _bibleRepository.GetBible().Books.FirstOrDefault(x => x.Id == bookId);

            var chapter = book.BookChapter.FirstOrDefault(x => x.ChapterId == chapterId);

            var readingContent = chapter.ChapterVerses.Aggregate("", (current, verse) =>
              current + string.Format("<p>{0}: {1} <p/>", verse.Id, verse.VerseText));

            var readingTitle = book.BookName + " \nChapter: " + chapter.ChapterId;

            return Json(new { readingTitle, chapterId, bookId, readingContent, status = true }, JsonRequestBehavior.AllowGet);

        }
         
        public ActionResult BibleAnimated()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chapterId"></param>
        /// <param name="bookId"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        [ActionName("next_previous")]
        public JsonResult GotoNextPrevious(int chapterId = 1, int bookId = 1, string direction = "")
        {
            if (direction == "previous")
            {
                if (chapterId == 1 && bookId != 1)
                {
                    bookId--;
                }
                else
                {
                    chapterId--;
                }
            }
            else
            {
                chapterId++;
            }

            var book = _bibleRepository.GetBible().Books.FirstOrDefault(x => x.Id == bookId);

            var chapter = book.BookChapter.FirstOrDefault(x => x.ChapterId == chapterId);

            var readingContent = chapter.ChapterVerses.Aggregate("", (current, verse) =>
              current + string.Format("<p>{0}: {1} <p/>", verse.Id, verse.VerseText));

            var readingTitle = book.BookName + " \nChapter: " + chapter.ChapterId;

            return Json(new { readingTitle, chapterId, bookId, readingContent, status = true }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chapterId"></param>
        /// <param name="bookId"></param>
        /// <returns></returns>
        public ActionResult Chapter(int chapterId = 1, int bookId = 1)
        {
            var book = _bibleRepository.GetBible().Books.FirstOrDefault(x => x.Id == bookId);

            var chapter = book.BookChapter.FirstOrDefault(x => x.ChapterId == chapterId);

            ViewBag.ReadingContent = chapter.ChapterVerses.Aggregate("", (current, verse) =>
              current + string.Format("<p>{0}: {1} <p/>", verse.Id, verse.VerseText));

            ViewBag.ReadingTitle = book.BookName + " \nChapter: " + chapter.ChapterId;

            ViewBag.ChapterId = chapterId;
            ViewBag.BookId = bookId;

            return View();
        }


        [ActionName("bible_portion")]
        public JsonResult GetSelectedDailyReading(int pageNumber = 1)
        {
            string readingResult;

            var model = _bibleRepository.GetBible();
            switch (pageNumber)
            {
                case 4:
                    readingResult = RenderPartialViewToString("_BookTableOfContent", model);
                    break;

                default:
                    readingResult = RenderPartialViewToString("_BookContent", model);
                    break;
            }

            return Json(new { BookContent = readingResult, status = true });

        }

    }
}
