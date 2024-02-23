function demoFromHTML() {
    var pdf = new jsPDF('p', 'pt', 'letter');
    source = $('#customers')[0];

    specialElementHandlers = {
        '#bypassme': function (element, renderer) {
            return true
        }
    };
    margins = {
        top: 80,
        bottom: 60,
        left: 40,
        width: 522
    };
    pdf.fromHTML(
    source,
    margins.left, 
    margins.top, { 
        'width': margins.width,
        'elementHandlers': specialElementHandlers
    },

    function (dispose) {
        //pdf.save('Test.pdf');

        var pdfDataUri = pdf.output('datauristring');
            var newWindow = window.open();
            newWindow.document.write('<style>body, html { margin: 0; padding: 0; }</style>');
            newWindow.document.write('<iframe width="100%" height="100%" style="margin: 0; padding: 0; border: none;" src="' + pdfDataUri + '"></iframe>');
        
            newWindow.document.title = 'Geothermal Report';
    }, margins);
}