/** @file xtalkshow-1.1.js
 *  @brief 播放互動腳本.

 *  在 xt_frames[] 陣列當中已經用 JavaScript Object 的方式寫出了互動的腳本，這裡的程式碼會播放它們，
 *  這個版本解決在 retina 螢幕上會模糊的問題，所以先把 canvas & context 依照 DPR 放大，再以 style 縮
 *  小為原本的尺寸。

 *  @author Shu-Kai Yang (http://www.cyberworlds.net/)
 *  @date 2024/10/12 */

var xt_music = true;
var xt_audio = true;
var xt_debug = false;

/* The canvas and 2D rendering context: */
var xt_canvas = null;
var xt_context = null;
var xt_canvas_width = 1512;
var xt_canvas_height = 850;

/* 暫存每個畫面都一定會用到的 DOM 元素參考: */
var xt_scene_img = null;
var xt_dialog_full_div = null;
var xt_dialog_div = null;
var xt_speaker_div = null;
var xt_episode_link_a = null;
var xt_next_a = null;

var xt_music_img = null;
var xt_audio_img = null;

/* 目前播放 frame number，初始為零，除非來自 URL 另以 xf 參數指定之: */
var xt_pos = 0;

/* 在 xtOnLoad() 中掃描所有 label 的位置: */
var xt_dict = new Object();

/* 這些陣列用來快取腳本中使用到的 img 與 audio 物件: */
var xt_imgs = new Object();
var xt_audios = new Object();

/* 目前播放中的背景音樂物件: */
var xt_curbgm = null;

/* 這個陣列儲存使用者的路徑選擇: */
var xt_ans = new Object();

/* 紀錄目前的背景圖檔，盡量避免在換畫面的時候重複載入: */
var xt_cur_scene = "";
var xt_cur_cover = "";

function xtGetChildByID(node, childID)
{
    for (let i=0; i<node.childNodes.length; ++i)
    {
        if (node.childNodes[i].id == childID)
        {   return node.childNodes[i];  }
    }

    return null;
}

/* 提供作為 音樂/音效 開關的處理常式: */
function xtOnMusicSwitch()
{
    if (xt_music == true)
    {
        xt_music = false;
        if (xt_curbgm != null) {  xt_curbgm.pause();  }

        if (xt_music_img != null)
        {   xt_music_img.src = "../Images/music_off.png";  }

        localStorage.setItem("xt_music", "false");
    }
    else
    {
        xt_music = true;
        if (xt_curbgm != null) {  xt_curbgm.play();  }

        if (xt_music_img != null)
        {   xt_music_img.src = "../Images/music_on.png";  }

        localStorage.setItem("xt_music", "true");
    }
}

function xtOnAudioSwitch()
{
    xt_audio = onOff;

    if (xt_audio == true)
    {
        xt_audio = false;

        if (xt_audio_img != null)
        {   xt_audio_img.src = "../Images/sound_off.png";  }

        localStorage.setItem("xt_audio", "false");
    }
    else
    {
        xt_audio = true;

        if (xt_audio_img != null)
        {   xt_audio_img.src = "../Images/sound_on.png";  }

        localStorage.setItem("xt_audio", "true");
    }
}

/* 當網頁載入的時候，在這個函式預先查詢所有的標籤位置以及資源物件，然後描繪第一個畫面。 */
function xtOnLoad()
{
    let start_frame = 0;
    let dpr = window.devicePixelRatio || 1;

    /* xt_width 和 xt_height 是必須定義的參數，在開始播放之前先自我檢查一下: */
    if (typeof xt_width === 'undefined') {  alert("Required variable xt_width is undefined!");  }
    else if (xt_width == 0)  {  alert("Required variable xt_width is zero!");  }

    if (typeof xt_height === 'undefined') {  alert("Required variable xt_height is undefined!");  }
    else if (xt_height == 0)  {  alert("Required variable xt_height is zero!");  }

    /* 掃描 xt_frame 陣列中所有 label 的位置並且放入 dict: */
    for (let i=0; i<xt_frames.length; ++i)
    {
        if (xt_frames[i].hasOwnProperty("label"))
        {
            if (xt_dict.hasOwnProperty(xt_frames[i].label) == false)
            {   xt_dict[xt_frames[i].label] = i;  }
        }
    }

    /* 檢查網址中是否有用 ?xf=n 或 ?xloc=label 來指定起始的播放位置: */
    let url = location.search;
    if(url.indexOf('?') != -1)
    {
        let args = url.split('?')[1].split('&');
        for(let i=0; i<args.length; ++i)
        {
            let arg = args[i].split('=');
            if (arg[0] == 'xdb')
            {   xt_debug = true;  }
            else if (arg[0] == 'xf')
            {   start_frame = parseInt(arg[1]);  }
            else if (arg[0] == 'xloc')
            {   if (arg[1] in xt_dict) {  start_frame = xt_dict[arg[1]];  };  }
            else if (arg[0] == 'dpr')
            {   dpr = parseFloat(arg[1]);  }
            /* 把剩下的網址參數放進 xt_ans，但是要先過濾社群或廣告追蹤變數:
            else
            {
                let key = arg[0].toLowerCase();
                if ((key.startsWith("fb") == false) &&
                    (key.startsWith("utm") == false) &&
                    (key.startsWith("ga_") == false))
                {   xt_ans[arg[0]] = arg[1];  }
            } */
        }
    }

    /* 掃描 DOM 中所有 class 為 xt_preloaded 的 img 元素: */
    let imgs = document.getElementsByClassName("xt_preloaded");
    for (let i=0; i<imgs.length; ++i) {  xt_imgs[imgs[i].id] = imgs[i];  }

    /* 掃描 DOM 中所有 class 為 xt_embedded 的 audio 元素: */
    let audios = document.getElementsByClassName("xt_embedded");
    for (let i=0; i<audios.length; ++i) {  xt_audios[audios[i].id] = audios[i];  }

    /* 尋找 DOM 中是否有作為 音樂/音效 開關的圖示: */
    let xt_music_img = document.getElementById("xt_music_img");
    let xt_audio_img = document.getElementById("xt_audio_img");

    /* 取得 canvas 元素並且取得 2d 畫布物件，為了解決 retina 螢幕上的模糊問題，先把 canvas 依照 DPR
       放大，再利用設定 CSS 的方式將 canvas 的成像縮小回原來的尺寸: */
    let container = document.getElementsByClassName("xt_container")[0];
    xt_canvas = xtGetChildByID(container, "xt_framebuffer");

    let rect = xt_canvas.getBoundingClientRect();
    xt_canvas_width = rect.width * dpr;
    xt_canvas_height = rect.height * dpr;
    xt_canvas.width = xt_canvas_width;
    xt_canvas.height = xt_canvas_height;
    xt_context = xt_canvas.getContext("2d");
    xt_context.scale(1, 1);

    xt_canvas.style.width = rect.width + 'px';
    xt_canvas.style.height = rect.height + 'px';

 /* if (typeof xt_context.imageSmoothingEnabled !== "undefined")
    {
        xt_context.imageSmoothingEnabled = true;
        if (typeof xt_context.imageSmoothingQuality !== "undefined")
        {   xt_context.imageSmoothingQuality = "high";  }
    } */

    /* 暫存每個畫面都一定會用到的 DOM 元素參考: */
    xt_scene_img       = xtGetChildByID(container, "xt_scene");
    xt_dialog_full_div = xtGetChildByID(container, "xt_dialog_full");
    xt_dialog_div      = xtGetChildByID(container, "xt_dialog");
    xt_speaker_div     = xtGetChildByID(container, "xt_speaker");
    xt_episode_a       = xtGetChildByID(container, "xt_episode_link");
    xt_next_a          = xtGetChildByID(container, "xt_next");

    /* 從 localStorage 取回目前的 音樂/音效 開關設定: */
    let str = localStorage.getItem("xt_music");
    if (str === "false")
    {
        xt_music = false;
        if (xt_music_img != null)
        {   xt_music_img.src = "../Images/music_off.png";  }
    }

    str = localStorage.getItem("xt_audio");
    if (str === "false")
    {
        xt_audio = false;
        if (xt_audio_img != null)
        {   xt_audio_img.src = "../Images/sound_off.png";  }
    }

    /* 準備完成，描繪第一個畫面: */
    xtRenderFrame(start_frame);
}

/* 由於把整個互動腳本用 JavaScript Object 的格式呈現，xt_frames 陣列中每一個物件都指示了這個畫面上應
 * 該要繪製的元素，包括背景圖檔、左邊角色、中間角色、右邊角色、對話框以及可點選的選項(或下一頁)按鈕。
 * 在 xtRenderFrame() 中會根據 xt_frames[i] 的物件成員決定那些畫面元素應該要顯現並繪製到 context。 */
function xtRenderFrame(i)
{
    /* 偵錯時在標題列顯示現在播放到第幾畫面: */
    let cur_frame = xt_frames[i];
    if (xt_debug == true)
    {
        if (cur_frame.hasOwnProperty("label"))
        {   document.title = "frame: " + cur_frame.label;  }
        else
        {   document.title = "frame: " + i;  }
    }

    /* 清除舊畫面: */
    xt_context.clearRect(0, 0, xt_canvas.width, xt_canvas.height);

    if (cur_frame.hasOwnProperty("darken"))
    {
        xt_context.globalAlpha = 0.3;
        xt_context.fillStyle = "black";
        xt_context.fillRect(0, 0, xt_canvas.width, xt_canvas.height);
        xt_context.globalAlpha = 1;
    }

    if (cur_frame.hasOwnProperty("blur"))
    {   xt_scene_img.style.filter = "blur(3px)";  }
    else
    {   xt_scene_img.style.filter = "none";  }

    /* 立即撥放過場音效或背影配樂: */
    if (cur_frame.hasOwnProperty("sound"))
    {
        let sound = xt_audios[cur_frame.sound];
        if (sound.loop == true)
        {
            if (xt_curbgm != null) {  xt_curbgm.pause();  }
            xt_curbgm = sound;
            if (xt_music == true) {  sound.play();  }
        }
        else if (xt_audio == true)
        {  sound.play();  }
    }

    if (cur_frame.hasOwnProperty("cover"))
    {
        if (cur_frame.cover != xt_cur_cover)
        {
            xt_cur_cover = cur_frame.cover;
            let cover_img = xtGetChildByID(xt_episode_a, "xt_episode");
            cover_img.src = xt_cur_cover;
        }

        /* 封面扉頁，所以隱藏封面影像以外的所有元素: */
        xt_episode_a.style.display = "block";
        xt_episode_a.focus();

        xt_scene_img.style.display = "none";
        xt_dialog_full_div.style.display = "none";
        xt_dialog_div.style.display = "none";
        xt_speaker_div.style.display = "none";
        xt_next_a.style.display = "none";
    }
    else
    {
        xt_episode_a.style.display = "none";
        xt_scene_img.style.display = "block";

        /* 更換背景影像: */
        if (cur_frame.hasOwnProperty("background"))
        {
            if (cur_frame.background != xt_cur_scene)
            {
                xt_cur_scene = cur_frame.background;
                xt_scene_img.src = xt_cur_scene;
            }
        }

        if (cur_frame.hasOwnProperty("full_text"))
        {
            /* 顯示全畫面對話框: */
            xt_dialog_full_div.style.display = "block";
            xt_dialog_div.style.display = "none";
            xt_speaker_div.style.display = "none";

            let xt_dialog_fulltext_span = xtGetChildByID(xt_dialog_full_div, "xt_dialog_fulltext");
            xt_dialog_fulltext_span.innerHTML = cur_frame.full_text;

            /* 顯示下一步按鈕或路徑選項，首先收集對話框內的 xt_option 元素: */
            let opts = [];
            for (let j=0; j<xt_dialog_full_div.childNodes.length; ++j)
            {
                if (xt_dialog_full_div.childNodes[j].className == "xt_option")
                {   opts.push(xt_dialog_full_div.childNodes[j]);  }
            }

            if (cur_frame.hasOwnProperty("options"))
            {
                xt_next_a.style.display = "none";

                for (let j=0; j<cur_frame.options.length; ++j)
                {
                    let option = cur_frame.options[j];
                    if (option.hasOwnProperty("text")) {  opts[j].innerHTML = option.text;  }
                    else {  opts[j].innerHTML = "";  }
                    opts[j].style.display = "block";
                }

                for (let j=cur_frame.options.length; j<opts.length; ++j)
                {   opts[j].style.display = "none";  }

             /* if (opts.length > 0) {  opts[0].focus();  } */
            }
            else
            {
                for (let j=0; j<opts.length; ++j)
                {   opts[j].style.display = "none";  }
                xt_next_a.style.display = "block";
                xt_next_a.focus();
            }
        }
        else
        {
            let base_y = 0;
            if (typeof xt_baseline !== 'undefined')
            {   base_y = (xt_baseline * xt_canvas_height) / xt_height;  }

            /* 顯示一般對話框: */
            xt_dialog_full_div.style.display = "none";
            xt_dialog_div.style.display = "block";

            if (cur_frame.hasOwnProperty("text"))
            {
                let xt_dialog_span = xtGetChildByID(xt_dialog_div, "xt_dialog_text");
                xt_dialog_span.innerHTML = cur_frame.text;
            }

            /* 描繪出場角色、計算影像的縮放比例與位置: */
            if (cur_frame.hasOwnProperty("center"))
            {
                let img = xt_imgs[cur_frame.center];
                let width = (img.width * xt_canvas_width) / xt_width;
                let height = (img.height * xt_canvas_height) / xt_height;

                let x = (xt_canvas_width - width) / 2;
                let y = xt_canvas_height - height - base_y;
                xt_context.drawImage(img, x, y, width, height);
            }

            if (cur_frame.hasOwnProperty("right2"))
            {
                let img = xt_imgs[cur_frame.right2];
                let width = (img.width * xt_canvas_width) / xt_width;
                let height = (img.height * xt_canvas_height) / xt_height;

                let x = (xt_canvas_width * 4) / 5 - width;
                let y = xt_canvas_height - height - base_y;
                xt_context.drawImage(img, x, y, width, height);
            }

            if (cur_frame.hasOwnProperty("left2"))
            {
                let img = xt_imgs[cur_frame.left2];
                let width = (img.width * xt_canvas_width) / xt_width;
                let height = (img.height * xt_canvas_height) / xt_height;

                let x = xt_canvas_width / 5;
                let y = xt_canvas_height - height - base_y;
                xt_context.drawImage(img, x, y, width, height);
            }

            if (cur_frame.hasOwnProperty("right"))
            {
                let img = xt_imgs[cur_frame.right];
                let width = (img.width * xt_canvas_width) / xt_width;
                let height = (img.height * xt_canvas_height) / xt_height;

                let x = xt_canvas_width * 0.97 - width;
                let y = xt_canvas_height - height - base_y;
                xt_context.drawImage(img, x, y, width, height);
            }

            if (cur_frame.hasOwnProperty("left"))
            {
                let img = xt_imgs[cur_frame.left];
                let width = (img.width * xt_canvas_width) / xt_width;
                let height = (img.height * xt_canvas_height) / xt_height;

                let x = xt_canvas_width / 33;
                let y = xt_canvas_height - height - base_y;
                xt_context.drawImage(img, x, y, width, height);
            }

            /* 顯示說話者的姓名框: */
            if (cur_frame.hasOwnProperty("speaker"))
            {
                xt_speaker_div.innerHTML = cur_frame.speaker;
                xt_speaker_div.style.display = "block";
            }
            else
            {   xt_speaker_div.style.display = "none";  }

            /* 顯示下一步按鈕或路徑選項，首先收集對話框內的 xt_option 元素: */
            let opts = [];
            for (let j=0; j<xt_dialog_div.childNodes.length; ++j)
            {
                if (xt_dialog_div.childNodes[j].className == "xt_option")
                {   opts.push(xt_dialog_div.childNodes[j]);  }
            }

            if (cur_frame.hasOwnProperty("options"))
            {
                xt_next_a.style.display = "none";

                for (let j=0; j<cur_frame.options.length; ++j)
                {
                    let option = cur_frame.options[j];
                    if (option.hasOwnProperty("text")) {  opts[j].innerHTML = option.text;  }
                    else {  opts[j].innerHTML = "";  }
                    opts[j].style.display = "block";
                }

                for (let j=cur_frame.options.length; j<opts.length; ++j)
                {   opts[j].style.display = "none";  }

             /* if (opts.length > 0) {  opts[0].focus();  } */
            }
            else
            {
                for (let j=0; j<opts.length; ++j)
                {   opts[j].style.display = "none";  }
                xt_next_a.style.display = "block";
                xt_next_a.focus();
            }
        }
    }


    /* 描繪完成，紀錄描繪位置: */
    xt_pos = i;
}

/* 可能是個 next 或 option 物件，並且根據它的屬性成員來決定去處。 */
function xtGotoPathOption(option)
{
    if (option.hasOwnProperty("label"))
    {
        let frame_index = xt_dict[option.label];
        xtRenderFrame(frame_index);
    }
    else
    {
        if (option.hasOwnProperty("url"))
        {
            let url = option.url;
            if ((option.hasOwnProperty("packopt")) && (option.packopt == "true"))
            {
                let firstParam = true;
                if(url.indexOf('?') != -1) {  firstParam = false;  }

                for (let key in xt_ans)
                {
                    let val = xt_ans[key];
                    if (firstParam == true)
                    {
                        url += ('?' + key + '=' + val);
                        firstParam = false;
                    }
                    else
                    {   url += ('&' + key + '=' + val);  }
                }
            }

            if (option.hasOwnProperty("target"))
            {
                if (option.target == "_blank")
                {   window.open(url);  }
                else if (option.target == "_top")
                {   top.location.href = url;  return;  }
                else
                {   window.location.href = url;  return;  }
            }
            else
            {   window.location.href = url;  return;  }
        }

        let frame_index = xt_pos +1;
        if (frame_index < xt_frames.length)
        {   xtRenderFrame(frame_index);  }
    }
}

function xtOnNextClicked()
{
    let cur_frame = xt_frames[xt_pos];
    if (cur_frame.hasOwnProperty("next"))
    {  xtGotoPathOption(cur_frame.next);  }
}

function xtOnOptionClicked(num)
{
    let cur_frame = xt_frames[xt_pos];
    if (cur_frame.hasOwnProperty("options"))
    {
        let option = cur_frame.options[num-1];
        if (option.hasOwnProperty("value"))
        {
            if ((typeof xt_optionConfirm !== 'undefined') && (xt_optionConfirm == true))
            {
                if (confirm("Are you sure?\n是否確定選擇這個選項?") == false) {  return;  }
            }

            if ((cur_frame.hasOwnProperty("label")) && (xt_ans.hasOwnProperty(cur_frame.label) == false))
            {
                xt_ans[cur_frame.label] = option.value;
            }

            xtGotoPathOption(option);

            if (xt_debug == true)
            {   document.title = cur_frame.label + " = " + option.value;  }
        }
        else
        {   xtGotoPathOption(option);  }
    }
}
