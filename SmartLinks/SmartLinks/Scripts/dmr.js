var DMR = function () {

    var show = function () {

        $('.date').datepicker({ autoclose: true, pickerPosition: "bottom-left", changeMonth: true,changeYear: true });

        var dmrdisttable = null;
        //var dmrsumtable = null;
        var dmrsntable = null;
        var snworkflowtable = null;

        var loadprodline = function () {
            $.post('/SmartLinks/LoadDRMProLine', {
            }, function (output) {
                console.log(output.prodlist);
                $('#proline').autoComplete({
                    minChars: 0,
                    source: function (term, suggest) {
                        term = term.toLowerCase();
                        var choices = output.prodlist;
                        var suggestions = [];
                        for (i = 0; i < choices.length; i++)
                            if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                        suggest(suggestions);
                    }
                });
                $('#proline').attr('readonly', false);
            })
        }

        $(function () {
            loadprodline();
        });

        var showtable = function (output) {

            if (dmrdisttable) {
                dmrdisttable.destroy();
                dmrdisttable = null;
            }
            $("#dmrdisthead").empty();
            $("#dmrdistcontent").empty();

            $("#dmrdisthead").append('<tr>' +
                    '<th>WORK FLOW STEP</th>' +
                    '<th>MODULE COUNT</th>' +
                    '</tr>');


            $.each(output.moduledist, function (i, val) {
                var appendstr = '<tr>';
                appendstr += '<td>' + val.WorkFlowStep + '</td>';
                appendstr += '<td>' + val.ModuleCount + '</td>';
                appendstr += '</tr>';
                $("#dmrdistcontent").append(appendstr);
            });

            dmrdisttable = $('#dmrdisttable').DataTable({
                'iDisplayLength': 50,
                'aLengthMenu': [[20, 50, 100, -1],
                [20, 50, 100, "All"]],
                "columnDefs": [
                    { "className": "dt-center", "targets": "_all" }
                ],
                "aaSorting": [],
                "order": [],
                dom: 'lBfrtip',
                buttons: ['copyHtml5', 'csv', 'excelHtml5']
            });




            if (dmrsntable) {
                dmrsntable.destroy();
                dmrsntable = null;
            }
            $("#dmrsnhead").empty();
            $("#dmrsncontent").empty();

            $("#dmrsnhead").append(
                '<tr style="font-size:12px;">' +
                    '<th>SN</th>' +
                    '<th>SN STAT</th>' +
                    '<th>DMR#</th>' +
                    '<th>DMR START</th>' +
                    '<th>DMR RETURN</th>' +
                    '<th>FAILURE</th>' +
                    '<th>CRT WFSTEP</th>' +
                    '<th>CRT WF</th>' +
                    '<th>RW FROM</th>' +
                    '<th>OA(D)</th>' +
                    '<th>DMR STORE(D)</th>' +
                    '<th>DMR REPAIR(D)</th>' +
                    '<th>DMR TOTAL(D)</th>' +
                '</tr>'
                );

            $.each(output.dmrdata, function (i, val) {
                var appendstr = '<tr style="font-size:10px;">';
                appendstr += '<td class="SNWORKFLOW" myid="' + val.SN + '"><strong>' + val.SN + '</strong></td>';
                appendstr += '<td>' + val.SNStatus + '</td>';
                appendstr += '<td>' + val.DMRID + '</td>';
                appendstr += '<td>' + val.DMRDate + '</td>';
                appendstr += '<td>' + val.DMRReturnTime + '</td>';
                appendstr += '<td>' + val.SNFailure + '</td>';
                appendstr += '<td>' + val.WorkFlowStep + '</td>';
                appendstr += '<td>' + val.WorkFlow + '</td>';
                appendstr += '<td>' + val.DMRRepairStep + '</td>';
                appendstr += '<td>' + val.OASpend + '</td>';
                appendstr += '<td>' + val.StoreSpend + '</td>';
                appendstr += '<td>' + val.RepairSpend + '</td>';
                appendstr += '<td>' + val.TotleDRMSpend + '</td>';
                appendstr += '</tr>';
                $("#dmrsncontent").append(appendstr);
            });


            dmrsntable = $('#dmrsntable').DataTable({
                'iDisplayLength': 50,
                'aLengthMenu': [[20, 50, 100, -1],
                [20, 50, 100, "All"]],
                "aaSorting": [],
                "order": [],
                dom: 'lBfrtip',
                buttons: ['copyHtml5', 'csv', 'excelHtml5']
            });
        }


        var DMRWIPFUN = function () {
            var prodline = $('#proline').val();
            if (prodline == '')
            {
                alert('Product Line  need to be input for WIP function!');
                return false;
            }

            var options = {
                loadingTips: "正在处理数据，请稍候...",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/SmartLinks/DMRWIPData',
                { prodline: prodline },
                function (output) {
                    $.bootstrapLoading.end();
                    showtable(output);
            })

        }

        $('body').on('click', '#btn-wip', function () {
            DMRWIPFUN();
        })


        var DMRTRACE = function () {
            var prodline = $('#proline').val();
            var sdate = $('#sdate').val();
            var edate = $('#edate').val();
            if (prodline == '' || sdate == '' || edate == '')
            {
                alert('Product Line, start date, end date need to be input for TRACE function!');
                return false;
            }

            var options = {
                loadingTips: "正在处理数据，请稍候...",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/SmartLinks/DMRTRACEData',
                {
                    prodline: prodline,
                    sdate: sdate,
                    edate: edate
                },
                function (output) {
                    $.bootstrapLoading.end();
                    showtable(output);
                })
        }

        $('body').on('click', '#btn-trace', function () {
            DMRTRACE();
        })



        $('body').on('click', '.SNWORKFLOW', function () {
            var sn = $(this).attr('myid');
            $.post('/SmartLinks/SNWholeWorkFlow', {
                sn: sn
            },
            function (output) {
                if (snworkflowtable) {
                    snworkflowtable.destroy();
                    snworkflowtable = null;
                }
                $("#snworkflowcontent").empty();

                $.each(output.snworkflowlist, function (i, val) {
                    var appendstr = '<tr>';
                    appendstr += '<td>' + val.SN + '</td>';
                    appendstr += '<td>' + val.PN + '</td>';
                    appendstr += '<td>' + val.WorkFlow + '</td>';
                    appendstr += '<td>' + val.WorkFlowStep + '</td>';
                    appendstr += '<td>' + val.JO + '</td>';
                    appendstr += '<td>' + val.DMRDate + '</td>';
                    appendstr += '</tr>';
                    $("#snworkflowcontent").append(appendstr);
                });

                snworkflowtable = $('#snworkflowtable').DataTable({
                    'iDisplayLength': 50,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "columnDefs": [
                        { "className": "dt-center", "targets": "_all" }
                    ],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['copyHtml5', 'csv', 'excelHtml5']
                });

                $('#snworkflowmodal').modal('show');
            });
        })



    }


    return {
        init: function () {
            show();
        }
    }
}();