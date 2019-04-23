var DMR = function () {

    var show = function () {

        $('.date').datepicker({ autoclose: true, pickerPosition: "bottom-left", changeMonth: true,changeYear: true });

        var dmrdisttable = null;
        var dmrsumtable = null;
        var dmrsntable = null;

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
                })
        }

        $('body').on('click', '#btn-trace', function () {
            DMRTRACE();
        })
    }


    return {
        init: function () {
            show();
        }
    }
}();