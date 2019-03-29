var JOWORKFLOW = function () {
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
            $.post('/SmartLinks/JOWorkFlowData',
           {
               marks: JSON.stringify(cur_marks)
           }, function (output) {
               $.bootstrapLoading.end();
               if (mywafertable) {
                   mywafertable.destroy();
               }
               $("#WaferTableID").empty();

               $.each(output.jdatalist, function (i, val) {
                   var appendstr = '<tr style="font-size:10px">';
                   appendstr += '<td>' + val.ContainerName + '</td>'
                   appendstr += '<td>' + val.SNStatus + '</td>'
                   appendstr += '<td>' + val.CustomerSerialNum + '</td>'
                   appendstr += '<td>' + val.MfgOrderName + '</td>'
                   appendstr += '<td>' + val.LotNum + '</td>'
                   appendstr += '<td>' + val.PackageNum + '</td>'
                   appendstr += '<td>' + val.ProductName + '</td>'
                   appendstr += '<td>' + val.CRTWFName + '</td>'
                   appendstr += '<td>' + val.CRTWFRev + '</td>'
                   appendstr += '<td>' + val.CRTWFStepName + '</td>'
                   appendstr += '<td>' + val.OrgWFName + '</td>'
                   appendstr += '<td>' + val.OrgWFRev + '</td>'
                   appendstr += '<td>' + val.OrgWFStepName + '</td>'
                   appendstr += '<td>' + val.HoldReason + '</td>'
                   appendstr += '<td>' + val.ScrapReason + '</td>'
                   appendstr += '<td>' + val.MfgDate + '</td>'
                   appendstr += '<td>' + val.Description + '</td>'
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
        init: function () {
            show();
        }
    }
}();