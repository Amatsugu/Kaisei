var authAppList, myApps, statsBox;
var icon, username, email, password;
var iconForm, iconProgress, upMsg;
$(document).ready(function(){
	authAppList = $("#authAppList");
	myApps = $("#myApps");
	iconForm = $("form")[0];
	iconProgress = $(".progress");
	icon = $("#changeIcon").click(function(e){
		
	}).prev().prev();
	upMsg = $("#uploadMessage");
	$("form input").dmUploader({
		url: "/user/icon",
		dnd: false,
		multiple:false,
		allowedTypes: "image/*",
		maxFileSize: 1e6,
		onUploadProgress: function (p) {
			iconProgress.css({
				width: p + "%"
			});
		},
		onUploadComplete: function () {
			console.log("complete");
			iconProgress.show();
			iconProgress.css({
				width: 100 + "%"
			});
			upMsg.text("Uploaded!");
			upMsg.fadeIn(500, function () {
				upMsg.delay(1000).fadeOut(500);
				iconProgress.delay(1000).fadeOut(500, function () {
					iconProgress.css({
						width: 0 + "%"
					});
				});
				icon.attr("src", "/user/avatar?m=" + new Date().getTime());
			});
		},
		onUploadError: function (xhr, status, err) {
			console.log("failed " + status);
			iconProgress.css({
				width: 100 + "%"
			});
			upMsg.text("Uploaded Failed");
			upMsg.fadeIn(500, function () {
				upMsg.delay(1000).fadeOut(500);
				iconProgress.delay(1000).fadeOut(500, function () {
					iconProgress.css({
						width: 0 + "%"
					});
				});
			});
		},
		onDragEnter: function () {
			upMsg.text = "Upload";
			upMsg.fadeIn(500);	
		},
		onDragLeave: function () {
			upMsg.fadeOut(500);	
		},
	});
	username = $("#username").click(function(e){

	}).next();
	email = $("#email").click(function(e){
		
	}).next();
	password = $("#password").click(function(e){
		
	}).next();
	statsBox = $("#statsBox");

	$.ajax({
		url: "/user/authedapps",
		method: "GET"
	}).done(function(e){
		// console.log(e);
		authAppList.text("");
		// if(e.length > 0)
		AddStat("Authorized Apps", e.length);
		ListApps(e, authAppList);
	}).fail(function(e){
		authAppList.text("Failed to load apps");
	});

	$.ajax({
		url: "user/apps",
		method: "GET"
	}).done(function(e){
		// console.log(e);
		myApps.text("");
		AddStat("My Apps", e.length);
		ListApps(e, myApps);
	}).fail(function(e){
		myApps.text("Failed to load apps");
	});;
});

function RevokeApp(appId, elem)
{
	$.ajax({
		url: "/user/revokeApp",
		method: "POST",
		data:{
			AppId : appId
		}
	}).done(function(e){
		var del = $("<div class='deleted' style='display:none'><p>Deleted</p></div>").appendTo(elem);
		del.fadeIn(500, function () {
			elem.delay(1000).slideUp(500, function () {
				elem.remove();
			});
		});
	}).fail(function(e){
		var del = $("<div class='deleted' style='display:none'><p>Failed to Delete</p></div>").appendTo(elem);
		del.fadeIn(500).delay(1000).fadeOut(500);
	});
}

function AddStat(name, value) 
{
	var statBox = $("<div class='statBox' style='display:none'></div>").appendTo(statsBox);
	statBox.append("<div class='statName'>"+ name +"</div>");
	statBox.append("<div class='stat'>"+ value +"</div>");
	statBox.fadeIn(500);
}

function ListApps(apps, container)
{
	if(apps.length == 0)
	{
		container.text("No apps to display...");
	}
	apps.forEach(app => {
		var appE = $("<div class='app' style='display:none'></div").appendTo(container);
		appE.append("<div class='name'>" + app.name + "</div>");
		appE.append("<div class='host'><a href='" + app.hostname + "'>" + app.hostname + "</a></div>");
		$("<button>Revoke</button>").appendTo(appE).click(function(){
			RevokeApp(app.id, appE);
		});
		appE.slideDown(500);
	});
}