﻿var SNPROGRESS = function () {
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

        function RefreshTable(qtype,warning) {
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

            var url = '/SmartLinks/SNWorkFlowData';
            if (qtype.indexOf('workflow') == -1)
            {
                url = '/SmartLinks/SNTestFlowData';
            }

            $.post(url,
               {
                   marks: JSON.stringify(cur_marks)
               }, function (output) {
                   $.bootstrapLoading.end();

               if (mywafertable) {
                   mywafertable.destroy();
                   mywafertable = null;
               }

               $("#waferheadid").empty();
               $("#WaferTableID").empty();
               var appendstr = '';
               
               if (qtype.indexOf('workflow') != -1) {
                   appendstr = '';
                   appendstr += '<tr>';
                   appendstr += '<th>SN</th>';
                   appendstr += '<th>PN</th>';
                   appendstr += '<th>WorkFlow</th>';
                   appendstr += '<th>TimeStamp</th>';
                   appendstr += '</tr>';
                   $("#waferheadid").append(appendstr);

                   
                   $.each(output.sndatalist, function (i, val) {
                       appendstr = '';
                       appendstr += '<tr>';
                       appendstr += '<td>' + val.SN + '</td>';
                       appendstr += '<td>' + val.PN + '</td>';
                       appendstr += '<td>' + val.WKFlow + '</td>';
                       appendstr += '<td>' + val.Time + '</td>';
                       appendstr += '</tr>';
                       $("#WaferTableID").append(appendstr);
                   });
                   
               } else {
                   appendstr = '';
                   appendstr += '<tr>';
                   appendstr += '<th>SN</th>';
                   appendstr += '<th>PN</th>';
                   appendstr += '<th>WhichTest</th>';
                   appendstr += '<th>Failure</th>';
                   appendstr += '<th>Tester</th>';
                   appendstr += '<th>TimeStamp</th>';
                   appendstr += '</tr>';
                   $("#waferheadid").append(appendstr);

                   
                   $.each(output.sndatalist, function (i, val) {
                       appendstr = '';
                       if (val.Failure.indexOf("PASS") != -1) {
                           appendstr += '<tr>';
                       }
                       else {
                           appendstr += '<tr class="tr-danger">';
                       }
                       appendstr += '<td>' + val.SN + '</td>';
                       appendstr += '<td>' + val.PN + '</td>';
                       appendstr += '<td>' + val.WKFlow + '</td>';
                       appendstr += '<td>' + val.Failure + '</td>';
                       appendstr += '<td>' + val.Tester + '</td>';
                       appendstr += '<td>' + val.Time + '</td>';
                       appendstr += '</tr>';
                       $("#WaferTableID").append(appendstr);
                   });
               }

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


        $('body').on('click', '#btn-marks-workflow', function () {
            RefreshTable('workflow', true);
        })

        $('body').on('click', '#btn-marks-testflow', function () {
            RefreshTable('testflow', true);
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