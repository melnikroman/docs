
var PhotoLikeManager = (function () {
	
	var MaxLikes = 3;
	var CountLikes = 0;
	var LikesListId;
	
	function init(maxLikes) {
		MaxLikes = maxLikes;
	}
	
	function setListId(listId) {
		LikesListId = "{" + listId + "}";
	}
	
	function addLikePhoto(photoId, remove) {
		var context = new SP.ClientContext();
		var list = context.get_web().get_lists().getById(LikesListId);
		
		var item;
		if (remove) {
			item = list.getItemById(photoId);
			item.deleteObject();
		} else {
			var itemCI = new SP.ListItemCreationInformation();
            item = list.addItem(itemCI);
			item.set_item("pspPictureId", photoId);
			item.update();
		}
		
		context.executeQueryAsync(null, function (sender, args) {
			console.log(args.get_message());
		});
	}
	
	function likePhoto(id) {
		$(".liked[data-id='" + id + "']").toggleClass(function () {
			if ($(this).hasClass("me")) {
				var dec = Number($(this).find("span b").text());
				$(this).find("span b").text(--dec);
				CountLikes--;
				
				addLikePhoto($(this).attr("data-remove-id"), true);
				return "me";
			} else {
				if (CountLikes >= MaxLikes)
					return "";
				
				var inc = Number($(this).find("span b").text());
				$(this).find("span b").text(++inc);
				CountLikes++;
				
				addLikePhoto(id, false);
				return "me";
			}
		});
	}
	
	return {
		init: init,
		setListId: setListId,
		likePhoto: likePhoto
	}
}());