using CommBiz.Api.Features.DirectEntry;

namespace CommBiz.Api.Tests;

public class DirectEntryHeaderRecordMapperTests
{
    private static ConvertDirectEntryBatchRequest ValidRequest() =>
        new(
            FileName: "COMPANY ABCD PTY LTD",
            UserIdentificationNumber: "301500",
            DescriptionOfEntries: "EFT-PAYMENT",
            DateToBeProcessed: new DateOnly(2026, 12, 5),
            Instructions: []);

    [Fact]
    public void Mapped_record_is_exactly_120_characters()
    {
        var record = DirectEntryHeaderRecordMapper.Map(ValidRequest());

        Assert.Equal(120, record.Length);
    }

    [Fact]
    public void Each_field_lands_at_its_documented_character_position()
    {
        var record = DirectEntryHeaderRecordMapper.Map(ValidRequest());

        Assert.Equal("0", record[0..1]); // Record Type, position 1, length 1
        Assert.Equal(new string(' ', 17), record[1..18]); // Blank, position 2-18, length 17
        Assert.Equal("01", record[18..20]); // Reel Sequence Number, position 19-20, length 2
        Assert.Equal("CBA", record[20..23]); // Name of User Financial Institution, position 21-23, length 3
        Assert.Equal(new string(' ', 7), record[23..30]); // Blank, position 24-30, length 7
        Assert.Equal("COMPANY ABCD PTY LTD", record[30..56].TrimEnd()); // File Name, position 31-56, length 26
        Assert.Equal("301500", record[56..62]); // User Identification Number, position 57-62, length 6
        Assert.Equal("EFT-PAYMENT", record[62..74].TrimEnd()); // Description of Entries, position 63-74, length 12
        Assert.Equal("051226", record[74..80]); // Date to be Processed, position 75-80, length 6
        Assert.Equal(new string(' ', 40), record[80..120]); // Blank, position 81-120, length 40
    }

    [Fact]
    public void File_name_shorter_than_max_is_left_justified_space_filled()
    {
        var record = DirectEntryHeaderRecordMapper.Map(ValidRequest() with { FileName = "AB" });

        Assert.Equal("AB" + new string(' ', 24), record[30..56]);
    }

    [Fact]
    public void File_name_at_max_length_needs_no_padding()
    {
        var name = new string('A', 26);
        var record = DirectEntryHeaderRecordMapper.Map(ValidRequest() with { FileName = name });

        Assert.Equal(name, record[30..56]);
    }

    [Fact]
    public void User_identification_number_shorter_than_6_digits_is_right_justified_zero_filled()
    {
        var record = DirectEntryHeaderRecordMapper.Map(ValidRequest() with { UserIdentificationNumber = "42" });

        Assert.Equal("000042", record[56..62]);
    }

    [Fact]
    public void Description_of_entries_shorter_than_max_is_left_justified_space_filled()
    {
        var record = DirectEntryHeaderRecordMapper.Map(ValidRequest() with { DescriptionOfEntries = "AB" });

        Assert.Equal("AB" + new string(' ', 10), record[62..74]);
    }

    [Fact]
    public void Date_with_single_digit_day_and_month_formats_as_ddmmyy_without_dropping_leading_zeros()
    {
        var record = DirectEntryHeaderRecordMapper.Map(
            ValidRequest() with { DateToBeProcessed = new DateOnly(2025, 1, 5) });

        Assert.Equal("050125", record[74..80]);
    }
}
