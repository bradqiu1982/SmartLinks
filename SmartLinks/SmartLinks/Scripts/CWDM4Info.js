var CWDM4Info = function () {
    var show = function () {
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
            $.post('/SmartLinks/GetCWDM4InfoData',
           {
               marks: JSON.stringify(cur_marks)
           }, function (output) {
               var idx = 0;
               var datacont = output.data.length;

               if (mywafertable) {
                   mywafertable.destroy();
               }
               $("#WaferTableID").empty();

               //$.each(output.cwdm4list, function (i, val) {
               //    var appendstr = '<tr>';
               //    appendstr += '<td>' + val.param + '</td>';
               //    appendstr += '<td>' + val.datafrom + '</td>';
               //    appendstr += '<td>' + val.isnormal + '</td>';
               //    appendstr += '<td>' + val.mean + '</td>';
               //    appendstr += '<td>' + val.stddev + '</td>';
               //    appendstr += '<td>' + val.realcpk + '</td>';
               //    appendstr += '<td>' + val.dppm + '</td>';
               //    appendstr += '<td>' + '<button class="btn btn-primary cpkmorecla" mydataid="' + i + '">More</button>' + '</td>';
               //    appendstr += '<td>' + '<a class="btn btn-primary" href="' + val.sourcedata + '" id="sourcedata" name="sourcedata" target="_blank">Source Data</a>' + '</td>';
               //    appendstr += '</tr>';
               //    $("#WaferTableID").append(appendstr);
               //});

               $.bootstrapLoading.end();
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
        init: function () {
            show();
        }
    }
}();