$(".modal").draggable({
    handle: ".modal-header"
  });
  
  $(".modal").on('shown.bs.modal', function (e) {
    $(".modal iframe").attr('src', "https://www.youtube-nocookie.com/embed/nEkiYKsYtqo?autoplay=1&amp;modestbranding=1&amp;showinfo=0&amp;start=0" ); 
  })
  
  $(".modal").on('hide.bs.modal', function (e) {
    $(".modal iframe").attr('src'," "); 
  })