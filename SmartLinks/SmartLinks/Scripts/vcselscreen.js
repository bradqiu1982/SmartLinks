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

    var snapinitfun = function ()
    {
        var mywafertable = null;

        $('#marks').focus();

        $('body').on('keypress', '#marks', function (e) {
            if (e.keyCode == 13) {
                var all_marks = $.trim($(this).val()).split('\n');
                var cur_marks = new Array();
                var arr_count = new Array();
                $.each(all_marks, function (i, val) {
                    if (val != "") {
                        if (arr_count[val]) {
                            alert(val + " has already existed.");
                            arr_count[val]++;
                        }
                        else {
                            arr_count[val] = 1;
                            cur_marks.push(val);
                        }
                    }
                })
                $('#total-marks').html(cur_marks.length);
                $('#marks').val(cur_marks.join('\n'));
            }
        })

        function RefreshWaferTable(warning) {

            var all_marks = $.trim($('#marks').val()).split('\n');
            var cur_marks = new Array();
            var arr_count = new Array();
            $.each(all_marks, function (i, val) {
                if (val != "") {
                    if (arr_count[val]) {
                        alert(val + " has already existed.");
                        arr_count[val]++;
                    }
                    else {
                        arr_count[val] = 1;
                        cur_marks.push(val);
                    }
                }
            })
            if (cur_marks.length === 0) {
                if (warning) {
                    alert("查询条件不可为空！");
                }
                return false;
            }
            $('#marks').val(cur_marks.join('\n'));
            var options = {
                loadingTips: "正在处理数据，请稍候...",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/SmartLinks/SNApertureSizeData',
           {
               marks: JSON.stringify(cur_marks)
           }, function (output) {
               $.bootstrapLoading.end();
               if (mywafertable) {
                   mywafertable.destroy();
               }
               $("#WaferTableID").empty();

               $.each(output.aplist, function (i, val) {
                   var appendstr = '<tr>';
                   appendstr += '<td>' + val.SN + '</td>';
                   appendstr += '<td>' + val.CH + '</td>';
                   appendstr += '<td>' + val.Wafer + '</td>';
                   appendstr += '<td>' + val.IthSlope + '</td>';
                   appendstr += '<td>' + val.Intercept + '</td>';
                   appendstr += '<td>' + val.Ith + '</td>';
                   appendstr += '<td>' + val.ApertureConst + '</td>';
                   appendstr += '<td>' + val.ApertureSize + '</td>';
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

           })
        }


        $('body').on('click', '#btn-marks-submit', function () {
            RefreshWaferTable(true);
        })

        $('body').on('click', '#btn-marks-clean', function () {
            $('#total-marks').html(0);
            $('#marks').val('');
            if (mywafertable) {
                mywafertable.destroy();
                mywafertable = null;
            }
            $("#WaferTableID").empty();
        })
    }

    var prepareapfun = function () {
        var mywafertable = null;

        $('#marks').focus();

        $('body').on('keypress', '#marks', function (e) {
            if (e.keyCode == 13) {
                var all_marks = $.trim($(this).val()).split('\n');
                var cur_marks = new Array();
                var arr_count = new Array();
                $.each(all_marks, function (i, val) {
                    if (val != "") {
                        if (arr_count[val]) {
                            alert(val + " has already existed.");
                            arr_count[val]++;
                        }
                        else {
                            arr_count[val] = 1;
                            cur_marks.push(val);
                        }
                    }
                })
                $('#total-marks').html(cur_marks.length);
                $('#marks').val(cur_marks.join('\n'));
            }
        })

        function RefreshWaferTable(warning) {

            var all_marks = $.trim($('#marks').val()).split('\n');
            var cur_marks = new Array();
            var arr_count = new Array();
            $.each(all_marks, function (i, val) {
                if (val != "") {
                    if (arr_count[val]) {
                        alert(val + " has already existed.");
                        arr_count[val]++;
                    }
                    else {
                        arr_count[val] = 1;
                        cur_marks.push(val);
                    }
                }
            })
            if (cur_marks.length === 0) {
                if (warning) {
                    alert("查询条件不可为空！");
                }
                return false;
            }
            $('#marks').val(cur_marks.join('\n'));
            var options = {
                loadingTips: "正在处理数据，请稍候...",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/SmartLinks/PrepareApertureSizeData',
           {
               marks: JSON.stringify(cur_marks)
           }, function (output) {
               $.bootstrapLoading.end();
               if (mywafertable) {
                   mywafertable.destroy();
               }
               $("#WaferTableID").empty();

               $.each(output.aplist, function (i, val) {
                   var appendstr = '<tr>';
                   appendstr += '<td>' + val.wf + '</td>';
                   appendstr += '<td>' + val.stat + '</td>';
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

           })
        }


        $('body').on('click', '#btn-marks-submit', function () {
            RefreshWaferTable(true);
        })

        $('body').on('click', '#btn-marks-clean', function () {
            $('#total-marks').html(0);
            $('#marks').val('');
            if (mywafertable) {
                mywafertable.destroy();
                mywafertable = null;
            }
            $("#WaferTableID").empty();
        })
    }

    return {
        INIT: function () {
            initfun();
        },
        SNAPINIT: function () {
            snapinitfun();
        },
        PREPAREAP: function () {
            prepareapfun();
        }
    };
}()