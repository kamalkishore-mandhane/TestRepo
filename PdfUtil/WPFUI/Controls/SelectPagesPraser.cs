using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;

namespace PdfUtil.WPFUI.Controls
{
    static class SelectPagesPraser
    {
        // Select pages parsing rules:
        //  1.Spaces around '-' are ignored, other spaces are treated as commas.
        //  2.Multiple consecutive spaces and/or commas are treated as a single comma.
        //  3.Page numbers higher than the last page are treated as the last page.
        //  4.Page numbers less than 1 are treated as 1. 
        //  5.Mis-ordered dash sequences are re-ordered.For example, "4-2" is treated as "2-4". 
        //  6.When reformatting user input for display, consecutive comma-separated pages can be treated as ranges. For example,
        //    the user input "2,3,4" can be displayed as "2-4". Duplicate numbers are ignored.
        static public List<int> PraseTextInTextbox(string selectText, int pageCount)
        {
            if (!string.IsNullOrEmpty(selectText))
            {
                selectText = Regex.Replace(selectText, @"\s*(-)\s*", "-");
                var pageArray = selectText.Split(new char[] { ',', ' '}, StringSplitOptions.RemoveEmptyEntries);
                if (pageArray != null)
                {
                    var pageList = new List<int>();

                    foreach (var pageNumItems in pageArray)
                    {
                        var items = pageNumItems.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                        var pageNums = OrganizePageNums(items, pageCount);
                        if (pageNums != null && pageNums.Length > 0)
                        {
                            for (int i = pageNums[0]; i <= pageNums[pageNums.Length - 1]; i++)
                            {
                                if (!pageList.Contains(i))
                                {
                                    pageList.Add(i);
                                }
                            }
                        }
                    }

                    pageList.Sort();
                    return pageList;
                }
            }
            return null;
        }

        static public string GenerareTextInTextbox(List<int> pageList)
        {
            if (pageList.Count > 0)
            {
                pageList.Sort();
                int index = 0;
                var selectPages = string.Empty;

                while (index < pageList.Count)
                {
                    var startPage = pageList[index++];
                    selectPages += startPage.ToString();

                    while (index < pageList.Count && pageList[index] == pageList[index - 1] + 1)
                    {
                        index++;
                    }

                    var endPage = pageList[--index];
                    selectPages += ((endPage - startPage >= 1) ? ('-' + endPage.ToString()) : "") + ',';
                    index++;
                }

                return selectPages.TrimEnd(',');
            }

            return null;
        }

        static private bool TryGetPageNum(string value, int pageCount, out int result)
        {
            if (int.TryParse(value, out result))
            {
                result = result > pageCount ? pageCount : result < 1 ? 1 : result;
                return true;
            }
            else
            {
                BigInteger bigPage;
                if (BigInteger.TryParse(value, out bigPage))
                {
                    result = pageCount;
                    return true;
                }
                return false;
            }
        }

        static private int[] OrganizePageNums(string[] pages, int pageCount)
        {
            List<int> PageNums = new List<int>();

            foreach (var page in pages)
            {
                int result;
                if (TryGetPageNum(page, pageCount, out result))
                {
                    PageNums.Add(result);
                }
            }

            PageNums.Sort();
            return PageNums.ToArray();
        }
    }
}
