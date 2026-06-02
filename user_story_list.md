# Danh sach User Story AISAM

Danh sach duoi day bam sat active backend codebase hien tai, `BACKEND_CODE_PLAN.md`, `AISAM-BE/docs/superpowers/CODEBASE_UPDATE.md` va scope MVP thuc te. Cac user story duoc tach thanh 3 nhan de viet detail va bam roadmap cho khop:

- `Da co trong codebase`: da co controller/service/repository/DTO/DI active trong backend.
- `Can hoan thien/nghiem thu`: da nam trong roadmap gan, hoac da co mot phan nhung can hardening, admin, test, docs, smoke.
- `Chua migrate`: chua active trong codebase hien tai, hoac de sau MVP.

## 1. Da co trong codebase

### US-01 - Dang ky tai khoan
**Mo ta:** La nguoi dung moi, toi muon dang ky tai khoan bang email va mat khau de bat dau su dung AISAM.

### US-02 - Dang nhap tai khoan
**Mo ta:** La nguoi dung, toi muon dang nhap bang email va mat khau de truy cap he thong.

### US-03 - Lam moi phien dang nhap
**Mo ta:** La nguoi dung, toi muon refresh access token de tiep tuc su dung he thong ma khong phai dang nhap lai ngay lap tuc.

### US-04 - Xem thong tin tai khoan hien tai
**Mo ta:** La nguoi dung da dang nhap, toi muon xem thong tin tai khoan cua minh de xac nhan trang thai truy cap.

### US-05 - Dang xuat thiet bi hien tai
**Mo ta:** La nguoi dung, toi muon dang xuat khoi phien hien tai de bao ve tai khoan.

### US-06 - Dang xuat tat ca thiet bi
**Mo ta:** La nguoi dung, toi muon ket thuc tat ca cac session de thu hoi truy cap tren moi thiet bi.

### US-07 - Xac minh email
**Mo ta:** La nguoi dung, toi muon xac minh email de kich hoat day du tai khoan va tang do tin cay bao mat.

### US-08 - Gui lai email xac minh
**Mo ta:** La nguoi dung, toi muon yeu cau gui lai email xac minh neu chua nhan duoc mail truoc do.

### US-09 - Quen mat khau
**Mo ta:** La nguoi dung, toi muon yeu cau dat lai mat khau khi khong con nho thong tin dang nhap.

### US-10 - Dat lai mat khau bang token
**Mo ta:** La nguoi dung, toi muon dat lai mat khau bang token hop le de khoi phuc quyen truy cap.

### US-11 - Dang nhap bang Google
**Mo ta:** La nguoi dung, toi muon dang nhap bang Google de vao he thong nhanh hon.

### US-12 - Tao business profile
**Mo ta:** La nguoi dung, toi muon tao business profile de tach du lieu van hanh theo doanh nghiep.

### US-13 - Xem danh sach profile cua toi
**Mo ta:** La nguoi dung, toi muon xem cac profile thuoc ve minh de chon ngu canh lam viec.

### US-14 - Cap nhat business profile
**Mo ta:** La nguoi dung, toi muon sua thong tin profile de phan anh dung doanh nghiep dang quan ly.

### US-15 - Quan ly brand kit
**Mo ta:** La nguoi dung, toi muon tao, xem, cap nhat, xoa va khoi phuc brand kit de cung cap ngu canh thuong hieu cho noi dung AI.

### US-16 - Gan profile scope cho brand
**Mo ta:** La he thong, toi muon chi cho phep truy cap brand thuoc profile dang hoat dong de dam bao ownership dung.

### US-17 - Quan ly san pham theo brand
**Mo ta:** La nguoi dung, toi muon tao, xem, cap nhat, xoa va khoi phuc san pham theo tung brand de quan ly du lieu quang cao.

### US-18 - Tim kiem va loc san pham
**Mo ta:** La nguoi dung, toi muon tim kiem va loc san pham theo brand va tu khoa de chon dung du lieu dau vao.

### US-19 - Tao noi dung thu cong
**Mo ta:** La nguoi dung, toi muon tao content draft theo profile, brand va product de bat dau quy trinh sang tao.

### US-20 - Quan ly thu vien noi dung
**Mo ta:** La nguoi dung, toi muon xem danh sach, chi tiet, cap nhat, xoa mem va khoi phuc content de quan ly ban nhap.

### US-21 - Nhan ban noi dung
**Mo ta:** La nguoi dung, toi muon clone mot content da co de tai su dung nhanh cho chien dich moi.

### US-22 - Sinh draft noi dung bang AI
**Mo ta:** La nguoi dung, toi muon AI sinh draft text dua tren brand, product va prompt de rut ngan thoi gian viet noi dung.

### US-23 - Cai thien content bang AI
**Mo ta:** La nguoi dung, toi muon AI improve mot content da co dua tren feedback de co phien ban tot hon.

### US-24 - Phe duyet ket qua AI de cap nhat content
**Mo ta:** La nguoi dung, toi muon chon mot AI generation phu hop va ap dung no vao content chinh.

### US-25 - Xem lich su AI generation cua content
**Mo ta:** La nguoi dung, toi muon xem danh sach cac generation da tao cho mot content de so sanh va chon lai.

### US-26 - Chat voi AI theo ngu canh profile
**Mo ta:** La nguoi dung, toi muon chat voi AI trong ngu canh profile, brand va product de brainstorming noi dung lien tuc.

### US-27 - Xem lich su hoi thoai AI
**Mo ta:** La nguoi dung, toi muon xem danh sach va chi tiet cac conversation de theo doi qua trinh lam viec voi AI.

### US-28 - Xoa hoi thoai AI
**Mo ta:** La nguoi dung, toi muon xoa conversation khong can thiet de giu lich su lam viec gon gang.

### US-29 - Ket noi Facebook qua OAuth
**Mo ta:** La nguoi dung, toi muon ket noi tai khoan Facebook de he thong co the truy cap cac page hop le.

### US-30 - Xem tai khoan social da lien ket
**Mo ta:** La nguoi dung, toi muon xem cac tai khoan social da ket noi trong profile hien tai de quan ly quyen truy cap.

### US-31 - Xem danh sach page co the dang bai
**Mo ta:** La nguoi dung, toi muon xem cac target/page kha dung de chon noi dung can ket noi voi brand.

### US-32 - Lien ket page voi brand
**Mo ta:** La nguoi dung, toi muon lien ket target Facebook voi brand de su dung cho publish.

### US-33 - Ngat ket noi tai khoan social hoac integration
**Mo ta:** La nguoi dung, toi muon xoa ket noi social hoac page integration de dung publish tren kenh do.

### US-34 - Dang bai ngay len Facebook
**Mo ta:** La nguoi dung, toi muon publish content len Facebook Page da lien ket de xuat ban bai viet truc tiep tu AISAM.

### US-35 - Xem lich su bai dang
**Mo ta:** La nguoi dung, toi muon xem danh sach va chi tiet cac bai da publish de theo doi ket qua xuat ban.

### US-36 - Nhan thong bao noi bo
**Mo ta:** La nguoi dung, toi muon nhan notification noi bo cho cac su kien he thong lien quan den workflow cua minh.

### US-37 - Xem danh sach va chi tiet thong bao
**Mo ta:** La nguoi dung, toi muon xem cac thong bao da nhan de theo doi su kien quan trong.

### US-38 - Danh dau thong bao da doc
**Mo ta:** La nguoi dung, toi muon danh dau mot thong bao hoac tat ca thong bao la da doc de quan ly hop thu noi bo.

### US-39 - Xem so thong bao chua doc
**Mo ta:** La nguoi dung, toi muon xem unread count de biet con su kien nao chua xu ly.

### US-40 - Dat lich dang bai mot lan
**Mo ta:** La nguoi dung, toi muon tao lich dang bai mot lan cho content de tu dong xuat ban dung thoi diem.

### US-41 - Quan ly lich dang bai
**Mo ta:** La nguoi dung, toi muon xem, cap nhat, xoa va theo doi upcoming schedules de dieu chinh ke hoach noi dung.

### US-42 - Tu dong dang bai theo lich
**Mo ta:** La he thong, toi muon background worker xu ly schedule den han de publish bai tu dong.

### US-43 - Xem dashboard tong quan MVP
**Mo ta:** La nguoi dung, toi muon xem dashboard summary theo profile de theo doi tong quan content, posts va scheduling.

### US-44 - Tao checkout subscription qua PayOS
**Mo ta:** La nguoi dung, toi muon tao checkout de thanh toan goi subscription cho profile cua minh.

### US-45 - Xem lich su thanh toan
**Mo ta:** La nguoi dung, toi muon xem lich su giao dich thanh toan de doi chieu cac lan nang cap goi.

### US-46 - Xem subscription hien tai
**Mo ta:** La nguoi dung, toi muon xem goi dang hoat dong cua profile de biet han muc va trang thai su dung.

### US-47 - Xu ly callback va webhook thanh toan
**Mo ta:** La he thong, toi muon nhan callback va webhook tu PayOS de dong bo trang thai thanh toan va subscription.

### US-48 - Xem tong quan quota theo profile
**Mo ta:** La nguoi dung, toi muon xem quota prompt va quota publish con lai de quan ly muc su dung.

### US-49 - Chan AI generation khi vuot quota
**Mo ta:** La he thong, toi muon tu choi AI request khi profile da het prompt quota de dung chinh sach subscription.

### US-50 - Chan publish khi vuot quota
**Mo ta:** La he thong, toi muon tu choi publish now hoac scheduled publish khi profile da het post quota.

## 2. Can hoan thien/nghiem thu

### US-51 - Dang nhap voi vai tro admin
**Mo ta:** La quan tri vien, toi muon dang nhap bang tai khoan admin de truy cap cac chuc nang quan tri rieng.

### US-52 - Xem danh sach nguoi dung trong admin
**Mo ta:** La quan tri vien, toi muon xem danh sach nguoi dung de quan ly van hanh he thong.

### US-53 - Quan ly profile, subscription va payment trong admin
**Mo ta:** La quan tri vien, toi muon xem va cap nhat mot so du lieu profile, subscription va payment de ho tro van hanh demo.

### US-54 - Seed du lieu demo duoc bao ve
**Mo ta:** La quan tri vien, toi muon tao du lieu demo bang endpoint duoc gioi han de phuc vu demo va kiem thu.

### US-55 - Chan truy cap admin voi non-admin
**Mo ta:** La he thong, toi muon enforce admin policy de nguoi dung thuong khong the dung endpoint quan tri.

### US-56 - Kiem thu ownership va boundary chinh
**Mo ta:** La nhom phat trien, toi muon co test cho auth, profile, brand, product, content, social, payment va scheduling de giam regression.

### US-57 - Tai lieu hoa setup va smoke test backend
**Mo ta:** La nhom phat trien, toi muon co setup guide va API smoke checklist de moi thanh vien co the chay demo on dinh.

## 3. Chua migrate

### US-58 - Approval workflow nang cao
**Mo ta:** La nguoi duyet noi dung, toi muon co quy trinh pending, approve, reject va feedback chinh thuc truoc khi publish.

### US-59 - Team va phan quyen team
**Mo ta:** La nguoi quan ly, toi muon tao team, gan thanh vien va phan quyen theo vai tro de to chuc cong viec theo nhom.

### US-60 - Facebook Ads MVP
**Mo ta:** La nguoi dung marketing, toi muon tao campaign, ad set, ad creative va ad tu content de mo rong sang quang cao tra phi.

### US-61 - Upload media qua storage service
**Mo ta:** La nguoi dung, toi muon upload va quan ly file media de dung trong product va content thay vi chi tham chieu URL thu cong.

### US-62 - Ket noi Instagram Business
**Mo ta:** La nguoi dung, toi muon ket noi Instagram Business de mo rong social publishing sau khi Facebook flow on dinh.

### US-63 - Ket noi TikTok Business
**Mo ta:** La nguoi dung, toi muon ket noi TikTok Business de mo rong pham vi social sau MVP.

### US-64 - Sinh anh AI day du
**Mo ta:** La nguoi dung, toi muon AI sinh anh quang cao hoan chinh cho content dang ImageText khi da san sang Vertex va storage.

### US-65 - Sinh video AI
**Mo ta:** La nguoi dung, toi muon AI tao video asset de ho tro content dang VideoText trong giai doan mo rong.

### US-66 - Quan ly plan dong
**Mo ta:** La quan tri vien, toi muon CRUD subscription plans dong de thay doi pricing va quota ma khong can sua code.

### US-67 - Ho tro analytics nang cao
**Mo ta:** La nguoi dung, toi muon xem analytics chi tiet hon theo kenh va chien dich de toi uu hieu qua marketing.

### US-68 - Ho tro AI recommendation va optimization
**Mo ta:** La nguoi dung, toi muon nhan de xuat chien luoc hoac toi uu tu dong sau khi MVP da on dinh.
