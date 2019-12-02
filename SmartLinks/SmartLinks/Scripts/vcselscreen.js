var VCSELSCREEN = function () {

    var initfun = function ()
    {
        var mywafertable = null;

        $.post('/Smartlinks/VcselScreenWafer', {
        }, function (output) {
            $('#wafernum').autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = output.wflist;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });
            $('#wafernum').attr('readonly', false);
        });

        function screenapsize() {
            $('#sw').text('');
            $('#sx').text('');
            $('#sy').text('');
            $('#sa').text('');
            $('#sp').text('');
            $('#result').empty();


            if (mywafertable) {
                mywafertable.destroy();
                mywafertable = null;
            }
            $("#tablename").text('');
            $("#WaferHead").empty();
            $("#WaferTableID").empty();

            var wf = $('#wafernum').val();
            var x = $('#x').val();
            var y = $('#y').val();
            if (wf == '' || x == '' || y == '')
            {
                alert("请输入全部查询条件！");
                return false;
            }

            var options = {
                loadingTips: "正在处理数据，请稍候...",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Smartlinks/VcselScreenData', {
                wf: wf,
                x: x,
                y: y
            }, function (output)
            {
                $.bootstrapLoading.end();

                $('#sw').text(output.wf);
                $('#sx').text(output.x);
                $('#sy').text(output.y);
                $('#sa').text(output.ar);
                //$('#sp').text(output.ap);
                $('#result').append(output.rest);

            });
        }
        
        $('body').on('click', '#btn-search', function () {
            screenapsize();
        });


        $('body').on('keypress', '#x', function (e) {
            if (e.keyCode == 13) {
                $('#y').val('');
                $('#y').focus();
            }
        });

        $('body').on('keypress', '#y', function (e) {
            if (e.keyCode == 13) {
                screenapsize();
            }
        });

        $('body').on('click', '#btn-hisdata', function () {
            var wf = $('#wafernum').val();
            var x = $('#x').val();
            var y = $('#y').val();
            if (wf == '') {
                alert("请输入WAFER NUMBER查询条件！");
                return false;
            }

            var options = {
                loadingTips: "正在处理数据，请稍候...",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Smartlinks/GetVcselScreenHistory', {
                wf: wf,
                x: x,
                y: y
            }, function (output) {
                $.bootstrapLoading.end();

                if (mywafertable) {
                    mywafertable.destroy();
                    mywafertable = null;
                }
                $("#tablename").text('VCSEL SCREEN HISTORY');
                $("#WaferHead").empty();
                $("#WaferTableID").empty();

                var appendstr = '<tr>';
                appendstr += '<th>Wafer Number</th>';
                appendstr += '<th>X</th>';
                appendstr += '<th>Y</th>';
                appendstr += '<th>ApSize</th>';
                appendstr += '<th>Array</th>';
                appendstr += '<th>Machine</th>';
                appendstr += '<th>Result</th>';
                appendstr += '<th>Update Time</th>';
                appendstr += '</tr>';
                $("#WaferHead").append(appendstr);

                $.each(output.hisdata, function (i, val) {
                    appendstr = '<tr>';
                    appendstr += '<td>' + val.Wafer + '</td>';
                    appendstr += '<td>' + val.X + '</td>';
                    appendstr += '<td>' + val.Y + '</td>';
                    appendstr += '<td>' + val.ApSize + '</td>';
                    appendstr += '<td>' + val.APVal1 + '</td>';
                    appendstr += '<td>' + val.APVal7 + '</td>';
                    appendstr += '<td>' + val.APVal8 + '</td>';
                    appendstr += '<td>' + val.APVal9 + '</td>';
                    appendstr += '</tr>';
                    $("#WaferTableID").append(appendstr);
                });

                mywafertable = $('#mywafertable').DataTable({
                    'iDisplayLength': 50,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['copyHtml5', 'csv', 'excelHtml5']
                });

            });
        });

    }

    return {
        INIT: function () {
            initfun();
        }
    };
}()