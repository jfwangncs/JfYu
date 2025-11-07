#if NET8_0_OR_GREATER
using JfYu.Office;
using JfYu.Office.Word;
using JfYu.Office.Word.Constant;
using JfYu.Office.Word.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NPOI.XWPF.UserModel;

namespace JfYu.UnitTests.Office.Word
{
    [Collection("Word")]
    public class JfYuWordTests
    {
        private readonly IJfYuWord _jfYuWord;
        public JfYuWordTests()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.AddJfYuWord().BuildServiceProvider();
            _jfYuWord = serviceProvider.GetRequiredService<IJfYuWord>();
        }


        private readonly string _testTemplatePath = "Static/template.docx";
        private readonly string _outputFilePath = "Static/output.docx";
        private readonly string _imagePath = "Static/example.png";
        private readonly string _imagePath1 = "Static/example1.png";

        [Fact]
        public void AddJfYuWord_Registers()
        {
            var services = new ServiceCollection();
            services.AddJfYuWord();

            var serviceProvider = services.BuildServiceProvider();

            var jfYuWord = serviceProvider.GetService<IJfYuWord>();

            Assert.NotNull(jfYuWord);
        }

        public class NullCreatePictureExpectData : TheoryData<XWPFRun?, JfYuWordReplacement?>
        {
            public NullCreatePictureExpectData()
            {
                using var doc = new XWPFDocument();
                var para = doc.CreateParagraph();
                var run = para.CreateRun();
                Add(null, new JfYuWordReplacement());
                Add(run, null);
            }
        }

        [Theory]
        [ClassData(typeof(NullCreatePictureExpectData))]
        public void CreatePicture_NullParameter_ThrowException(XWPFRun run, JfYuWordReplacement replacement)
        {
            var ex = Record.Exception(() => JfYuWordExtension.CreatePicture(run, replacement));
            Assert.IsAssignableFrom<ArgumentNullException>(ex);
        }

        [Fact]
        public void CreatePicture_NoReplace_Correctly()
        {
            var testString = "te1111tdadadadadadad";
            using var doc = new XWPFDocument();
            var para = doc.CreateParagraph();
            var run = para.CreateRun();
            run.SetText(testString);
            var pic = new JfYuWordReplacement() { Key = "test", Value = new JfYuWordPicture() { Width = 100, Height = 100, Bytes = File.ReadAllBytes(_imagePath) } };
            JfYuWordExtension.CreatePicture(run, pic);
            var run1 = para.CreateRun();
            pic = new JfYuWordReplacement() { Key = "test", Value = new JfYuWordPicture() { Width = 100, Height = 100, Bytes = File.ReadAllBytes(_imagePath1) } };
            JfYuWordExtension.CreatePicture(run1, pic);
            Assert.Equal(testString, string.Join("", para.Runs.SelectMany(q => q.Text)));
            Assert.Equal(2, doc.AllPictures.Count);
        }

        [Fact]
        public void CreatePicture_Replace_Correctly()
        {
            var testString = "te1111t{HelloWorld}dadadadadadad";
            using var doc = new XWPFDocument();
            var para = doc.CreateParagraph();
            var run = para.CreateRun();
            run.SetText(testString);
            var pic = new JfYuWordReplacement() { Key = "HelloWorld", Value = new JfYuWordPicture() { Width = 100, Height = 100, Bytes = File.ReadAllBytes(_imagePath) } };
            JfYuWordExtension.CreatePicture(run, pic);
            var run1 = para.CreateRun();
            pic = new JfYuWordReplacement() { Key = "HelloWorld", Value = new JfYuWordPicture() { Width = 100, Height = 100, Bytes = File.ReadAllBytes(_imagePath1) } };
            JfYuWordExtension.CreatePicture(run1, pic);
            Assert.Equal("te1111tdadadadadadad", string.Join("", para.Runs.SelectMany(q => q.Text)));
            Assert.Equal(2, doc.AllPictures.Count);
        }

        [Theory]
        [InlineData("")]
        [InlineData("notfound.docx")]
        public void GenerateWordByTemplate_TemplateEmpty_ThrowException(string templatePath)
        {
            var ex = Record.Exception(() => _jfYuWord.GenerateWordByTemplate(templatePath, _outputFilePath, []));
            Assert.IsAssignableFrom<FileNotFoundException>(ex);
        }

        [Fact]
        public void GenerateWordByTemplate_ShouldReplaceTextAndInsertImage()
        {
            var replacements = new List<JfYuWordReplacement>
            {
                new() { Key = "name", Value = new JfYuWordString() { Text = "Mia" } },
                new() { Key = "date", Value = new JfYuWordString() { Text = "2025-03-04" } },
                new() { Key = "logo", Value = new JfYuWordPicture() { Width = 100, Height = 100, Bytes = File.ReadAllBytes(_imagePath) } },
                new() { Key = "logo2", Value = new JfYuWordPicture() { Width = 100, Height = 100, Bytes = File.ReadAllBytes(_imagePath1) } }
            };

            _jfYuWord.GenerateWordByTemplate(_testTemplatePath, _outputFilePath, replacements);

            Assert.True(File.Exists(_outputFilePath));

            using var fs = new FileStream(_outputFilePath, FileMode.Open, FileAccess.Read);
            using var document = new XWPFDocument(fs);

            bool hasName = false;
            bool hasDate = false;
            bool hasImage = false;

            foreach (XWPFParagraph paragraph in document.Paragraphs)
            {
                string text = paragraph.Text;
                if (text.Contains("Mia")) hasName = true;
                if (text.Contains("2025-03-04")) hasDate = true;

                foreach (XWPFPicture picture in paragraph.Runs.SelectMany(r => r.GetEmbeddedPictures()))
                {
                    hasImage = true;
                }
            }

            Assert.True(hasName);
            Assert.True(hasDate);
            Assert.True(hasImage);
        }
    }

}
#endif