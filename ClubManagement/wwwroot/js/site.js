// Hiệu ứng giao diện cho ClubManagement (chỉ trình bày, không gọi dữ liệu, không đổi nghiệp vụ).
// Chuyển thể từ các mẫu chuyển động của Animate UI: highlight trượt, đếm số, hiện dần khi cuộn, đèn rọi.
(function () {
    "use strict";
    var reduced = window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    var fine = window.matchMedia && window.matchMedia("(hover: hover) and (pointer: fine)").matches;

    function ready(fn) {
        if (document.readyState !== "loading") fn(); else document.addEventListener("DOMContentLoaded", fn);
    }

    /* 1. Highlight trượt giữa các mục (Animate UI Highlight) */
    function initGlide(root) {
        var items = Array.prototype.slice.call(root.querySelectorAll("a"));
        if (!items.length) return;
        var pill = document.createElement("span");
        pill.className = "glide-pill no-anim";
        pill.setAttribute("aria-hidden", "true");
        if (getComputedStyle(root).position === "static") root.style.position = "relative";
        root.insertBefore(pill, root.firstChild);

        function place(el) {
            var r = root.getBoundingClientRect(), b = el.getBoundingClientRect();
            pill.style.setProperty("--gx", (b.left - r.left) + "px");
            pill.style.setProperty("--gy", (b.top - r.top) + "px");
            pill.style.width = b.width + "px";
            pill.style.height = b.height + "px";
        }
        function active() {
            return root.querySelector("a.active") || null;
        }
        function rest() {
            var a = active();
            if (a && a.offsetParent !== null) { place(a); pill.classList.add("is-on"); }
            else pill.classList.remove("is-on");
        }

        items.forEach(function (el) {
            if (el.classList.contains("nav-home-back")) return;
            el.addEventListener("pointerenter", function (e) {
                if (e.pointerType === "touch") return;
                place(el); pill.classList.add("is-on");
            });
            el.addEventListener("focus", function () { place(el); pill.classList.add("is-on"); });
            el.addEventListener("blur", rest);
        });
        root.addEventListener("pointerleave", rest);
        window.addEventListener("resize", rest);

        // Đặt vị trí ban đầu không animation (và sau khi phông tải xong)
        rest();
        requestAnimationFrame(function () { requestAnimationFrame(function () { pill.classList.remove("no-anim"); }); });
        if (document.fonts && document.fonts.ready) document.fonts.ready.then(function () {
            pill.classList.add("no-anim"); rest();
            requestAnimationFrame(function () { pill.classList.remove("no-anim"); });
        });
    }

    /* 2. Highlight cho các danh sách dạng hàng (tiết mục, thao tác nhanh) */
    function initRowGlide(root, selector) {
        var rows = Array.prototype.slice.call(root.querySelectorAll(selector));
        if (!rows.length || !fine || reduced) return;
        var pill = document.createElement("span");
        pill.className = "glide-pill";
        pill.setAttribute("aria-hidden", "true");
        root.insertBefore(pill, root.firstChild);
        rows.forEach(function (el) {
            el.addEventListener("pointerenter", function () {
                var r = root.getBoundingClientRect(), b = el.getBoundingClientRect();
                pill.style.setProperty("--gx", (b.left - r.left - root.clientLeft) + "px");
                pill.style.setProperty("--gy", (b.top - r.top - root.clientTop) + "px");
                pill.style.width = b.width + "px";
                pill.style.height = b.height + "px";
                pill.classList.add("is-on");
            });
        });
        root.addEventListener("pointerleave", function () { pill.classList.remove("is-on"); });
    }

    /* 3. Đếm số (Animate UI Counting Number) */
    function initCounters() {
        var nodes = document.querySelectorAll("[data-count]");
        if (!nodes.length || reduced || !("IntersectionObserver" in window)) return;
        function format(n) { return String(n); }
        function run(el) {
            var to = parseInt(el.getAttribute("data-count"), 10);
            if (isNaN(to) || to <= 0) return;
            var dur = Math.min(1600, 700 + to * 12), t0 = null;
            function step(t) {
                if (t0 === null) t0 = t;
                var p = Math.min(1, (t - t0) / dur);
                var eased = 1 - Math.pow(1 - p, 4); // ease-out
                el.textContent = format(Math.round(to * eased));
                if (p < 1) requestAnimationFrame(step); else el.textContent = format(to);
            }
            el.textContent = "0";
            requestAnimationFrame(step);
        }
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (en) { if (en.isIntersecting) { run(en.target); io.unobserve(en.target); } });
        }, { threshold: 0.4 });
        nodes.forEach(function (n) { io.observe(n); });
    }

    /* 4. Hiện dần khi cuộn, so le theo nhóm (Animate UI Fade/Slide) */
    function initReveal() {
        if (reduced || !("IntersectionObserver" in window)) return;
        var targets = document.querySelectorAll("[data-reveal]");
        if (!targets.length) return;
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (en) {
                if (en.isIntersecting) { en.target.classList.add("is-in"); io.unobserve(en.target); }
            });
        }, { threshold: 0.12, rootMargin: "0px 0px -6% 0px" });
        var groups = new Map();
        targets.forEach(function (t) {
            var g = t.parentElement;
            var i = groups.get(g) || 0; groups.set(g, i + 1);
            t.style.setProperty("--reveal-delay", Math.min(i, 5) * 80 + "ms");
            t.classList.add("reveal-ready");
            io.observe(t);
        });
    }

    /* 5. Đèn rọi sân khấu ở hero: theo con trỏ, tự quét khi không có chuột */
    function initSpotlight() {
        var hero = document.querySelector(".hero");
        if (!hero) return;
        var tx = 68, ty = 38, cx = tx, cy = ty, lastMove = 0, raf = null, visible = true;
        if (reduced) return;
        hero.addEventListener("pointermove", function (e) {
            if (e.pointerType === "touch") return;
            var r = hero.getBoundingClientRect();
            tx = ((e.clientX - r.left) / r.width) * 100;
            ty = ((e.clientY - r.top) / r.height) * 100;
            lastMove = performance.now();
        });
        if ("IntersectionObserver" in window) {
            new IntersectionObserver(function (en) { visible = en[0].isIntersecting; if (visible && !raf) loop(); }, { threshold: 0 }).observe(hero);
        }
        function loop(now) {
            raf = null;
            if (!visible) return;
            now = now || performance.now();
            if (now - lastMove > 2500) { // tự quét như đèn rọi
                tx = 62 + Math.sin(now / 2600) * 24;
                ty = 38 + Math.cos(now / 3300) * 14;
            }
            cx += (tx - cx) * 0.08; cy += (ty - cy) * 0.08;
            hero.style.setProperty("--mx", cx.toFixed(2) + "%");
            hero.style.setProperty("--my", cy.toFixed(2) + "%");
            raf = requestAnimationFrame(loop);
        }
        loop();
    }

    /* 6. Thanh điều hướng đổi nền khi cuộn (trang chủ) */
    function initHeader() {
        var header = document.querySelector(".page-home .site-header");
        if (!header) return;
        function on() { header.classList.toggle("is-stuck", window.scrollY > 40); }
        on(); window.addEventListener("scroll", on, { passive: true });
    }

    /* 7. Menu quản trị trên màn hình nhỏ */
    function initSidebar() {
        var btn = document.getElementById("sidebarToggle");
        var side = document.getElementById("adminSidebar");
        if (!btn || !side) return;
        function set(open) {
            document.body.classList.toggle("nav-open", open);
            btn.setAttribute("aria-expanded", open ? "true" : "false");
        }
        btn.addEventListener("click", function () { set(!document.body.classList.contains("nav-open")); });
        document.addEventListener("keydown", function (e) { if (e.key === "Escape") set(false); });
        document.addEventListener("click", function (e) {
            if (document.body.classList.contains("nav-open") && !side.contains(e.target) && !btn.contains(e.target)) set(false);
        });
        side.querySelectorAll("a").forEach(function (a) { a.addEventListener("click", function () { set(false); }); });
    }

    /* 8. Đóng menu công khai sau khi chọn (di động) */
    function initPublicNav() {
        document.querySelectorAll("#navbarContent .nav-link").forEach(function (link) {
            link.addEventListener("click", function () {
                var menu = document.getElementById("navbarContent");
                if (menu && menu.classList.contains("show") && window.bootstrap) {
                    window.bootstrap.Collapse.getOrCreateInstance(menu).hide();
                }
            });
        });
    }

    /* 9. Đánh số hàng bảng để hiện so le */
    function initTableStagger() {
        document.querySelectorAll(".table tbody").forEach(function (tb) {
            Array.prototype.forEach.call(tb.rows, function (tr, i) { tr.style.setProperty("--row", i); });
        });
    }

    ready(function () {
        document.querySelectorAll("[data-glide]").forEach(initGlide);
        document.querySelectorAll("[data-row-glide]").forEach(function (r) { initRowGlide(r, r.getAttribute("data-row-glide")); });
        initCounters(); initReveal(); initSpotlight(); initHeader();
        initSidebar(); initPublicNav(); initTableStagger();
    });
})();
