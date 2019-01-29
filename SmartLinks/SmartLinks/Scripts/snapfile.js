var SNAPFILE = function () {
    var snapinit = function () {

        $.post('/FileShare/ShareToList', {
        }, function (output) {
            $('#shareto').autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = output.sharetolist;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });
        });

        $.post('/FileShare/ShareToList', {}, function (output) {
            $('#shareto').tagsinput({
                freeInput: false,
                typeahead: {
                    source: output.sharetolist,
                    minLength: 0,
                    showHintOnFocus: true,
                    autoSelect: false,
                    selectOnBlur: false,
                    changeInputOnSelect: false,
                    changeInputOnMove: false,
                    afterSelect: function (val) {
                        this.$element.val("");
                    }
                }
            });
        });

        $('body').on('click', '#searchbtn', function () {
            var uploadfilename = $('#filename').html().toUpperCase();
            if (uploadfilename == '')
            {
                alert("You need to choose a file to share!");
                return false;
            }

            if (uploadfilename.indexOf(".TXT") == -1
                && uploadfilename.indexOf(".PDF") == -1
                && uploadfilename.indexOf(".DOC") == -1
                && uploadfilename.indexOf(".XLS") == -1
                && uploadfilename.indexOf(".PPT") == -1
                && uploadfilename.indexOf(".MSG") == -1
                && uploadfilename.indexOf(".HTML") == -1) {
                alert("At current time,we support following file format:.TXT .PDF .DOC .XLS .PPT .MSG .HTML");
                return false;
            }

            var shareto = $.trim($('#shareto').tagsinput('items'));
            if (shareto == '') {
                shareto = $.trim($('#shareto').parent().find('input').eq(0).val());
            }
            if (shareto == '')
            {
                alert("You need to choose at least one person to share your file!");
                return false;
            }
            
            $('#uploadform').submit();

        });

        $('body').on('click', '.DocLink', function () {
            var docid = $(this).attr('mydataid');
            window.open('/FileShare/ReviewDocument?docid=' + docid, "_blank");
        });

        $('body').on('click', '.DelLink', function () {
            var docid = $(this).attr('mydataid');
            if (confirm("Do you really want to remove this file?"))
            {
                $.post('/FileShare/RemoveDocument',
                    {
                        docid:docid
                    },
                    function (output) {
                        window.location.reload(true);
                    });
            }
        });

        var reviewtable = null;
        var loadreviewdocuments = function ()
        {
            $.post('/FileShare/RetrieveReviewDocuments', {}, function (output) {
                if (reviewtable) {
                    reviewtable.destroy();
                }

                $('#reviewtabhead').empty();
                $('#reviewtabcontent').empty();

                var appendstr = '<tr>' +
                                '<th>Review File</th>' +
                            '</tr>';
                $('#reviewtabhead').append(appendstr);

                $.each(output.doclist, function (i, val) {
                    appendstr = '<tr>' +
                        '<td><span class="DocLink" mydataid="' + val.DocID + '">' + val.FileAddr + '<span></td>' +
                        '</tr>';
                    $('#reviewtabcontent').append(appendstr);
                });

                reviewtable = $('#reviewtable').DataTable({
                    'iDisplayLength': 50,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lfrtip'
                });
            })
        }

        var sharetable = null;
        var loadsharedocuments = function () {
            $.post('/FileShare/RetrieveShareDocuments', {}, function (output) {
                if (sharetable) {
                    sharetable.destroy();
                }

                $('#sharetabhead').empty();
                $('#sharetabcontent').empty();

                var appendstr = '<tr>' +
                                '<th>Shared File</th>' +
                                '<th><span class="glyphicon glyphicon-trash"></span></th>'+
                            '</tr>';
                $('#sharetabhead').append(appendstr);

                $.each(output.doclist, function (i, val) {
                    appendstr = '<tr>' +
                        '<td><span class="DocLink" mydataid="' + val.DocID + '">' + val.FileAddr + '<span></td>' +
                        '<td><span class="glyphicon glyphicon-trash DelLink" mydataid="' + val.DocID + '"><span></td>' +
                        '</tr>';
                    $('#sharetabcontent').append(appendstr);
                });

                sharetable = $('#sharetable').DataTable({
                    'iDisplayLength': 50,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lfrtip'
                });
            })
        }

        $(function () {
            loadreviewdocuments();
        });

        $(function () {
            loadsharedocuments();
        });
    }

    return {
        INIT: function () {
            snapinit();
        }
    }
}();