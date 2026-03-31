/* ============================================================
   KAIJENSON MOTOR SHOP — Global JavaScript Utilities
   ============================================================ */

// ── Toast Notification System ──
(function () {
    let toastContainer = null;

    function ensureContainer() {
        if (!toastContainer) {
            toastContainer = document.getElementById('toastContainer');
            if (!toastContainer) {
                toastContainer = document.createElement('div');
                toastContainer.id = 'toastContainer';
                toastContainer.className = 'toast-container';
                document.body.appendChild(toastContainer);
            }
        }
        return toastContainer;
    }

    window.showToast = function (message, type) {
        type = type || 'success';
        const container = ensureContainer();
        const toast = document.createElement('div');
        toast.className = 'toast toast-' + type;

        const icons = {
            success: 'fa-check-circle',
            error: 'fa-exclamation-circle',
            info: 'fa-info-circle',
            warning: 'fa-exclamation-triangle'
        };

        toast.innerHTML =
            '<i class="fas ' + (icons[type] || icons.info) + '"></i>' +
            '<span>' + message + '</span>' +
            '<button class="toast-close" onclick="this.parentElement.remove()"><i class="fas fa-xmark"></i></button>';

        container.appendChild(toast);

        // Auto-remove after 4 seconds
        setTimeout(function () {
            if (toast.parentElement) {
                toast.style.animation = 'toastSlideOut 0.3s ease forwards';
                setTimeout(function () { toast.remove(); }, 300);
            }
        }, 4000);
    };
})();

// ── Export to Excel (requires SheetJS/xlsx) ──
window.exportToExcel = function (data, sheetName, fileName, colWidths) {
    if (typeof XLSX === 'undefined') {
        showToast('Excel library not loaded', 'error');
        return;
    }
    var ws = XLSX.utils.json_to_sheet(data);
    if (colWidths) {
        ws['!cols'] = colWidths.map(function (w) { return { wch: w }; });
    }
    var wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, sheetName || 'Sheet1');
    XLSX.writeFile(wb, fileName || 'Export.xlsx');
    showToast(sheetName + ' exported as Excel');
};

// Multi-sheet export
window.exportToExcelMulti = function (sheets, fileName) {
    if (typeof XLSX === 'undefined') {
        showToast('Excel library not loaded', 'error');
        return;
    }
    var wb = XLSX.utils.book_new();
    sheets.forEach(function (s) {
        var ws = XLSX.utils.json_to_sheet(s.data);
        if (s.colWidths) {
            ws['!cols'] = s.colWidths.map(function (w) { return { wch: w }; });
        }
        XLSX.utils.book_append_sheet(wb, ws, s.name);
    });
    XLSX.writeFile(wb, fileName || 'Export.xlsx');
    showToast('Report exported as Excel');
};

// ── Bulk Select Helpers ──
window.BulkSelect = {
    selectedIds: new Set(),

    toggle: function (id, checkbox) {
        if (checkbox.checked) {
            this.selectedIds.add(id);
        } else {
            this.selectedIds.delete(id);
        }
        this.updateUI();
    },

    toggleAll: function (headerCheckbox, selector) {
        var checkboxes = document.querySelectorAll(selector || '.row-checkbox');
        var self = this;
        checkboxes.forEach(function (cb) {
            cb.checked = headerCheckbox.checked;
            var id = cb.getAttribute('data-id');
            if (headerCheckbox.checked) {
                self.selectedIds.add(id);
            } else {
                self.selectedIds.delete(id);
            }
        });
        this.updateUI();
    },

    selectAll: function (ids) {
        var self = this;
        ids.forEach(function (id) { self.selectedIds.add(String(id)); });
        document.querySelectorAll('.row-checkbox').forEach(function (cb) {
            cb.checked = true;
        });
        var headerCb = document.querySelector('.header-checkbox');
        if (headerCb) headerCb.checked = true;
        this.updateUI();
    },

    deselectAll: function () {
        this.selectedIds.clear();
        document.querySelectorAll('.row-checkbox').forEach(function (cb) {
            cb.checked = false;
        });
        var headerCb = document.querySelector('.header-checkbox');
        if (headerCb) headerCb.checked = false;
        this.updateUI();
    },

    updateUI: function () {
        var bar = document.getElementById('bulkActionBar');
        var countEl = document.getElementById('selectedCount');
        var totalEl = document.getElementById('filteredCount');
        var deleteCountEl = document.getElementById('deleteSelectedCount');

        if (bar) {
            bar.style.display = this.selectedIds.size > 0 ? 'flex' : 'none';
        }
        if (countEl) countEl.textContent = this.selectedIds.size;
        if (deleteCountEl) deleteCountEl.textContent = this.selectedIds.size;

        // Highlight selected rows
        document.querySelectorAll('.row-checkbox').forEach(function (cb) {
            var row = cb.closest('tr');
            if (row) {
                row.classList.toggle('row-selected', cb.checked);
            }
        });
    },

    getIds: function () {
        return Array.from(this.selectedIds);
    },

    reset: function () {
        this.selectedIds.clear();
        this.updateUI();
    }
};

// ── Modal Helpers ──
window.openModal = function (modalId) {
    var modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'flex';
        document.body.style.overflow = 'hidden';
    }
};

window.closeModal = function (modalId) {
    var modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'none';
        document.body.style.overflow = '';
    }
};

// Close modal on overlay click
document.addEventListener('click', function (e) {
    if (e.target.classList.contains('modal-overlay')) {
        e.target.style.display = 'none';
        document.body.style.overflow = '';
    }
});

// ── Print Receipt ──
window.printReceipt = function (orderData) {
    var itemsHTML = '';
    if (orderData.items && orderData.items.length) {
        orderData.items.forEach(function (item) {
            var subtotal = (item.price * item.quantity);
            itemsHTML += '<tr>' +
                '<td style="padding:4px 0;font-size:12px;">' + item.productName + '</td>' +
                '<td style="padding:4px 0;font-size:12px;text-align:center;">' + item.quantity + '</td>' +
                '<td style="padding:4px 0;font-size:12px;text-align:right;">₱' + subtotal.toLocaleString('en-PH',{minimumFractionDigits:2}) + '</td>' +
                '</tr>';
        });
    }

    var printWindow = window.open('', '_blank', 'width=400,height=600');
    if (!printWindow) {
        showToast('Please allow popups for printing', 'error');
        return;
    }

    printWindow.document.write('<!DOCTYPE html>' +
        '<html><head><title>Order ' + orderData.id + '</title>' +
        '<style>body{font-family:"Courier New",monospace;padding:20px;max-width:300px;margin:0 auto;color:#333}' +
        '.center{text-align:center}.divider{border-top:1px dashed #ccc;margin:10px 0}table{width:100%;border-collapse:collapse}' +
        '@media print{body{margin:0;padding:10px}}</style></head><body>' +
        '<div class="center"><h2 style="margin:0;font-size:16px;">Kaijenson Motor Shop</h2>' +
        '<p style="margin:4px 0;font-size:11px;color:#666;">Order Receipt</p></div>' +
        '<div class="divider"></div>' +
        '<p style="font-size:12px;margin:4px 0;"><strong>Order:</strong> #' + orderData.id + '</p>' +
        '<p style="font-size:12px;margin:4px 0;"><strong>Date:</strong> ' + orderData.date + '</p>' +
        '<p style="font-size:12px;margin:4px 0;"><strong>Customer:</strong> ' + orderData.customer + '</p>' +
        '<p style="font-size:12px;margin:4px 0;"><strong>Payment:</strong> ' + orderData.payment + '</p>' +
        '<p style="font-size:12px;margin:4px 0;"><strong>Status:</strong> ' + orderData.status + '</p>' +
        '<div class="divider"></div>' +
        '<table><thead><tr style="font-size:11px;color:#666;">' +
        '<th style="text-align:left;padding-bottom:6px;">Item</th>' +
        '<th style="text-align:center;padding-bottom:6px;">Qty</th>' +
        '<th style="text-align:right;padding-bottom:6px;">Amount</th></tr></thead>' +
        '<tbody>' + itemsHTML + '</tbody></table>' +
        '<div class="divider"></div>' +
        '<table><tr style="font-weight:bold;font-size:14px;"><td>TOTAL</td>' +
        '<td style="text-align:right;">₱' + parseFloat(orderData.total).toLocaleString('en-PH',{minimumFractionDigits:2}) + '</td></tr></table>' +
        '<div class="divider"></div>' +
        '<div class="center"><p style="font-size:11px;color:#666;">Thank you for your purchase!</p></div>' +
        '</body></html>');

    printWindow.document.close();
    printWindow.focus();
    setTimeout(function () { printWindow.print(); }, 300);
};

// ── AJAX Helper ──
window.postJSON = function (url, data) {
    var token = document.querySelector('input[name="__RequestVerificationToken"]');
    return fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token ? token.value : ''
        },
        body: JSON.stringify(data)
    }).then(function (r) { return r.json(); });
};

// ── Format Helpers ──
window.formatPeso = function (amount) {
    return '₱' + parseFloat(amount).toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
};

window.formatNumber = function (num) {
    return parseFloat(num).toLocaleString('en-PH');
};

// ── Confirmation Dialog ──
window.confirmAction = function (message, callback) {
    if (confirm(message)) {
        callback();
    }
};

// ── Date Helpers ──
window.todayISO = function () {
    return new Date().toISOString().split('T')[0];
};

// ── Notification Bell System ──
(function () {
    var dropdown = null;
    var badge = null;
    var list = null;
    var summary = null;
    var isOpen = false;

    function init() {
        dropdown = document.getElementById('notificationDropdown');
        badge = document.getElementById('notificationBadge');
        list = document.getElementById('notificationList');
        summary = document.getElementById('notificationSummary');

        if (badge) {
            loadUnreadCount();
            // Poll every 30 seconds
            setInterval(loadUnreadCount, 30000);
        }

        // Close dropdown on outside click
        document.addEventListener('click', function (e) {
            if (dropdown && isOpen) {
                var bell = document.getElementById('notificationBell');
                if (!dropdown.contains(e.target) && e.target !== bell && !bell.contains(e.target)) {
                    dropdown.classList.remove('show');
                    isOpen = false;
                }
            }
        });
    }

    function loadUnreadCount() {
        fetch('/Notification/GetUnreadCount')
            .then(function (r) { return r.json(); })
            .then(function (data) {
                if (badge) {
                    if (data.count > 0) {
                        badge.textContent = data.count > 99 ? '99+' : data.count;
                        badge.style.display = 'flex';
                    } else {
                        badge.style.display = 'none';
                    }
                }
            })
            .catch(function () { });
    }

    function loadNotifications() {
        if (!list) return;
        list.innerHTML = '<div class="notification-loading"><i class="fas fa-spinner fa-spin"></i> Loading...</div>';

        fetch('/Notification/GetNotifications')
            .then(function (r) { return r.json(); })
            .then(function (data) {
                if (!data || data.length === 0) {
                    list.innerHTML = '<div class="notification-empty"><i class="fas fa-bell-slash"></i><p>No notifications</p></div>';
                    if (summary) summary.textContent = '0 notifications';
                    return;
                }

                var html = '';
                var typeIcons = {
                    'low-stock': 'fa-exclamation-triangle',
                    'new-order': 'fa-shopping-cart',
                    'daily-sales': 'fa-chart-line',
                    'system': 'fa-gear',
                    'customer': 'fa-user-plus'
                };
                var typeColors = {
                    'low-stock': 'notif-warning',
                    'new-order': 'notif-success',
                    'daily-sales': 'notif-info',
                    'system': 'notif-info',
                    'customer': 'notif-primary'
                };

                data.forEach(function (n) {
                    var icon = typeIcons[n.type] || 'fa-bell';
                    var colorClass = typeColors[n.type] || 'notif-info';
                    var readClass = n.isRead ? 'notification-read' : '';

                    html += '<div class="notification-item ' + readClass + '" data-id="' + n.notificationId + '">' +
                        '<div class="notification-item-icon ' + colorClass + '">' +
                        '<i class="fas ' + icon + '"></i>' +
                        '</div>' +
                        '<div class="notification-item-content">' +
                        '<p class="notification-item-title">' + n.title + '</p>' +
                        '<p class="notification-item-message">' + (n.message || '') + '</p>' +
                        '<span class="notification-item-time">' + n.timeAgo + '</span>' +
                        '</div>' +
                        (!n.isRead ? '<button class="notification-item-mark" onclick="markNotificationRead(' + n.notificationId + ', this)" title="Mark as read"><i class="fas fa-check"></i></button>' : '') +
                        '</div>';
                });

                list.innerHTML = html;
                var unreadCount = data.filter(function (n) { return !n.isRead; }).length;
                if (summary) summary.textContent = data.length + ' notifications' + (unreadCount > 0 ? ' (' + unreadCount + ' unread)' : '');
            })
            .catch(function () {
                list.innerHTML = '<div class="notification-empty"><i class="fas fa-exclamation-circle"></i><p>Failed to load</p></div>';
            });
    }

    window.toggleNotificationDropdown = function () {
        if (!dropdown) return;
        isOpen = !isOpen;
        dropdown.classList.toggle('show');
        if (isOpen) {
            loadNotifications();
        }
    };

    window.markNotificationRead = function (id, btn) {
        fetch('/Notification/MarkAsRead/' + id, { method: 'POST' })
            .then(function (r) { return r.json(); })
            .then(function () {
                var item = btn.closest('.notification-item');
                if (item) {
                    item.classList.add('notification-read');
                    btn.remove();
                }
                loadUnreadCount();
            })
            .catch(function () { });
    };

    window.markAllNotificationsRead = function () {
        fetch('/Notification/MarkAllRead', { method: 'POST' })
            .then(function (r) { return r.json(); })
            .then(function () {
                loadNotifications();
                loadUnreadCount();
            })
            .catch(function () { });
    };

    // Initialize when DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();

// ── Profile Dropdown Toggle ──
(function () {
    var profileDropdown = null;
    var isOpen = false;

    window.toggleProfileDropdown = function () {
        profileDropdown = document.getElementById('profileDropdown');
        if (!profileDropdown) return;
        isOpen = !isOpen;
        profileDropdown.classList.toggle('show', isOpen);
        var chevron = document.querySelector('.profile-chevron');
        if (chevron) chevron.style.transform = isOpen ? 'rotate(180deg)' : '';
    };

    document.addEventListener('click', function (e) {
        profileDropdown = document.getElementById('profileDropdown');
        if (!profileDropdown || !isOpen) return;
        var trigger = document.getElementById('profileDropdownTrigger');
        if (!trigger.contains(e.target)) {
            isOpen = false;
            profileDropdown.classList.remove('show');
            var chevron = document.querySelector('.profile-chevron');
            if (chevron) chevron.style.transform = '';
        }
    });
})();

// ── Global Search System ──
(function () {
    var searchInput, dropdown, resultsEl, loadingEl, emptyEl;
    var debounceTimer = null;

    function init() {
        searchInput = document.getElementById('globalSearchInput');
        dropdown = document.getElementById('globalSearchDropdown');
        resultsEl = document.getElementById('globalSearchResults');
        loadingEl = document.getElementById('globalSearchLoading');
        emptyEl = document.getElementById('globalSearchEmpty');

        if (!searchInput) return;

        searchInput.addEventListener('input', function () {
            clearTimeout(debounceTimer);
            var q = this.value.trim();
            if (q.length < 2) {
                hideDropdown();
                return;
            }
            debounceTimer = setTimeout(function () { performSearch(q); }, 300);
        });

        searchInput.addEventListener('focus', function () {
            if (this.value.trim().length >= 2 && resultsEl && resultsEl.innerHTML.trim()) {
                dropdown.classList.add('show');
            }
        });

        // Close on outside click
        document.addEventListener('click', function (e) {
            var wrapper = document.getElementById('globalSearchWrapper');
            if (wrapper && !wrapper.contains(e.target)) {
                hideDropdown();
            }
        });

        // Enter key submits to product search
        searchInput.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                hideDropdown();
                searchInput.blur();
            }
        });
    }

    function performSearch(q) {
        if (!dropdown) return;
        dropdown.classList.add('show');
        loadingEl.style.display = 'block';
        emptyEl.style.display = 'none';
        resultsEl.innerHTML = '';

        fetch('/Home/GlobalSearch?q=' + encodeURIComponent(q))
            .then(function (r) { return r.json(); })
            .then(function (data) {
                loadingEl.style.display = 'none';
                var html = '';
                var hasResults = false;

                // Products
                if (data.products && data.products.length > 0) {
                    hasResults = true;
                    html += '<div class="global-search-category">' +
                        '<span class="global-search-category-label"><i class="fas fa-boxes-stacked"></i> Products</span>';
                    data.products.forEach(function (p) {
                        html += '<a href="/Product/Details/' + p.productId + '" class="global-search-item">' +
                            '<div class="global-search-item-main">' +
                            '<strong>' + escapeHtml(p.productName) + '</strong>' +
                            '<span class="global-search-item-sub">' + escapeHtml(p.category) + ' · ₱' + p.price + '</span>' +
                            '</div>' +
                            '<span class="status-badge status-' + p.status.toLowerCase().replace(' ', '-') + '">' + p.status + '</span>' +
                            '</a>';
                    });
                    html += '</div>';
                }

                // Orders
                if (data.orders && data.orders.length > 0) {
                    hasResults = true;
                    html += '<div class="global-search-category">' +
                        '<span class="global-search-category-label"><i class="fas fa-clipboard-list"></i> Orders</span>';
                    data.orders.forEach(function (o) {
                        html += '<a href="/Sale/Orders" class="global-search-item">' +
                            '<div class="global-search-item-main">' +
                            '<strong>#' + o.saleId + ' — ' + escapeHtml(o.customerName) + '</strong>' +
                            '<span class="global-search-item-sub">₱' + o.total + ' · ' + o.date + '</span>' +
                            '</div>' +
                            '<span class="status-badge status-' + o.status.toLowerCase() + '">' + o.status + '</span>' +
                            '</a>';
                    });
                    html += '</div>';
                }

                // Customers
                if (data.customers && data.customers.length > 0) {
                    hasResults = true;
                    html += '<div class="global-search-category">' +
                        '<span class="global-search-category-label"><i class="fas fa-users"></i> Customers</span>';
                    data.customers.forEach(function (c) {
                        html += '<a href="/Customer" class="global-search-item">' +
                            '<div class="global-search-item-main">' +
                            '<strong>' + escapeHtml(c.name) + '</strong>' +
                            '<span class="global-search-item-sub">' + (c.email || 'No email') + ' · ₱' + c.totalPurchases + '</span>' +
                            '</div>' +
                            '</a>';
                    });
                    html += '</div>';
                }

                if (hasResults) {
                    resultsEl.innerHTML = html;
                    emptyEl.style.display = 'none';
                } else {
                    resultsEl.innerHTML = '';
                    emptyEl.style.display = 'flex';
                }
            })
            .catch(function () {
                loadingEl.style.display = 'none';
                emptyEl.style.display = 'flex';
            });
    }

    function hideDropdown() {
        if (dropdown) dropdown.classList.remove('show');
    }

    function escapeHtml(str) {
        if (!str) return '';
        var div = document.createElement('div');
        div.appendChild(document.createTextNode(str));
        return div.innerHTML;
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();

// ── Settings Tab from Session Storage ──
document.addEventListener('DOMContentLoaded', function () {
    var tab = sessionStorage.getItem('openSettingsTab');
    if (tab && typeof switchSettingsTab === 'function') {
        sessionStorage.removeItem('openSettingsTab');
        // Simulate click on the correct tab button
        var tabBtns = document.querySelectorAll('.settings-tab');
        tabBtns.forEach(function (btn) {
            if (btn.textContent.trim().toLowerCase().includes(tab)) {
                btn.click();
            }
        });
    }
});
